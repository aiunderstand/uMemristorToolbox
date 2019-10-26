/*
 * firmware16bitInputOutput.c
 *
 * Created: 08/10/2019 11:39:08
 * Author : Steven Bos, Richard Anthony
 */ 

/* 
 * OUTPUT: COLUMNS port C 0-7 (arduino pins digital 30-37) and ROWS port A (arduino pins digital 22-29) for 8x8 LED matrix with following mapping:
 * https://circuitdigest.com/fullimage?i=circuitdiagram_mic/ATmega8-LED-Matrix-Circuit.gif
 * http://cool-web.de/raspberry/mit-zwei-74hc595-und-drei-steuerleitungen-8x8-led-matrix-ansteuern.htm
 * We enable a row that drain the power to ground and then power the col.
 * ROWS	(on external pulldown) LED Pin	COLS                      lED PIN
 * AVR A0 (Arduino digital 22) : 9		AVR C0 (Arduino digital 37) : 13
 * AVR A1 (Arduino digital 23) : 14		AVR C1 (Arduino digital 36) : 3
 * AVR A2 (Arduino digital 24) : 8		AVR C2 (Arduino digital 35) : 4
 * AVR A3 (Arduino digital 25) : 12		AVR C3 (Arduino digital 34) : 10
 * AVR A4 (Arduino digital 26) : 1		AVR C4 (Arduino digital 33) : 6
 * AVR A5 (Arduino digital 27) : 7		AVR C5 (Arduino digital 32) : 11
 * AVR A6 (Arduino digital 28) : 2		AVR C6 (Arduino digital 31) : 15
 * AVR A7 (Arduino digital 29) : 5		AVR C7 (Arduino digital 30) : 16
 *
 * INPUT : port L 0-7 (arduino pins digital 42-49) for 4x4 keypad * 
 * - Keypad row pins are set low one at a time
 * - Keypad column pins are checked: 
 *		- normally high
 *		- low signals a key press at corresponding row / column crosspoint, note that the Keyboard handler logic must be inverted  I.e. a key press reads as a '0' on the relevant column.
 * - Activates one row at a time, then scans each column within active row
 * - Number pressed on keypad is updated in matrix
 *
 * COLS (on internal pullUP resistor) ROWS
 * AVR L0 (Arduino digital 42) :4     AVR L4 (Arduino digital 46)  :3
 * AVR L1 (Arduino digital 43) :5       AVR L5 (Arduino digital 47):2
 * AVR L2 (Arduino digital 44) :6      AVR L6 (Arduino digital 48) :1
 * AVR L3 (Arduino digital 45) :7      AVR L7 (Arduino digital 49) :0
 *
 * The 16 Keypad keys form a cross-connect matrix (4 Columns X 4 Rows)
 * Matrix Mapping
 * 		(4 Columns)			C0 	C1	C2 C3
 *		(4 Rows)		R0	1	2	3  4
 *						R1	5	6	7  8
 *						R2	9	10	11 12
 *						R3	13	14	15 16
 *
 *
 * Computing the Key_Value from its row and column positions:
 *		
 *		A. The key matrix positions are enumerated 1(0x00) - 16(0x0F)
 *			'1' = 0x00, '2' = 0x01, '3' = 0x02 '4' = 0x03
 *			'5' = 0x04, '6' = 0x05, '7' = 0x06 '8' = 0x07
 *			'9' = 0x08, '10' = 0x09, '11' = 0x0A '12' = 0x0B
 *			'13' = 0x0C, '14' = 0x0D, '15' = 0x0E '16' = 0x0F
 *		
 *		B.	The Key_Value is given the initial value of 1,5,9 or 13 (0x0C) 
 *			based on the row being scanned.
 *		
 *		C. The Key_Value is incremented once if the key detected is in Column 0
 *			(values 1, 5, 9, 13)
 *			The Key_Value is incremented twice if the key detected is in Column 1
 *			(values 2, 6, 10, 14)
 *			The Key_Value is incremented thrice if the key detected is in Column 2
 *			(values 3, 7, 11, 15)
 *			The Key_Value is incremented quadrice if the key detected is in Column 3
 *			(values 4, 8, 12, 16)
 *
 *
 * OUTPUT: port H 3-5 (arduino pins PWM 6-8) RGB-LED for when using internal (RED) or external (GREEN) state matrix 
 * INPUT : port E 4 (arduino pins PWM 2) button for toggling between external and internal state matrices
 *  
 * PROGRAM PSEUDO CODE
 * 
 * SETUP
 *      start with empty stateMatrix
 *		start serial com
 *
 * LOOP
 *		scan for changes in keypad buttons compared, if found update internal stateMatrix and send internal stateMatrix to serial port
 *      scan for changes in serial port, if found update externalStateMatrix
 *      updates LED based on internal stateMatrix or external stateMatrix  * 
 * 
 */ 


#include <avr/io.h>
#include <avr/interrupt.h>

#define F_CPU 1000000UL
#include <util/delay.h>

#include <string.h>
#define CR 0x0D
#define LF 0x0A 
#define CS 0x2C //ASCII 44: the commma ',' as seperation symbol in serial message
#define SS 0x24 //ASCII 44: the dollar '$' as start symbol in serial message
#define ES 0x3B //ASCII 44: the semicolon ';' as terminal symbol in serial message


//DEFINE MACRO's
/* https://www.electro-tech-online.com/threads/defining-and-using-bit-flags-in-c.140777/ */
#define sbi(b,n) ((b) |= (1<<(n)))          /* Set bit number n in byte b */
#define cbi(b,n) ((b) &= (~(1<<(n))))       /* Clear bit number n in byte b   */

// Set up a boolean variable type
#define TRUE 1
#define FALSE 0
typedef unsigned char bool;
//END DEF MACRO's

#define ScanKeypadRow0 0b01111111	// Bits 4-7 pulled low depending on row being scanned, bits 0-3 (pullups) remain high at all times
#define ScanKeypadRow1 0b10111111
#define ScanKeypadRow2 0b11011111
#define ScanKeypadRow3 0b11101111

#define KeypadMaskColumns 0b11110000
#define KeypadMaskColumn0 0b00001000
#define KeypadMaskColumn1 0b00000100
#define KeypadMaskColumn2 0b00000010
#define KeypadMaskColumn3 0b00000001

#define NoKey 0xFF

#define LED_RED 0b00001000
#define LED_GREEN 0b00011000

volatile bool useExternalMatrix;
volatile bool showPixel;
volatile int frameId = 0;
volatile int reducedBrightness = 3; //light up LED every n frame (causes flickering)

#define MAXSIZE 10 //typical format is = "$0,16,27;" - "startsymbol, messagetypeId, position, value, end symbol" so 9 char's
char inboundMsg[MAXSIZE];
volatile bool msgCompleted = FALSE;
volatile unsigned char msgCaret =0; // used for inboudMsg

char data[8][8] = {
					{0,0,0,0,0,0,0,0},
					{0,0,1,0,0,1,0,0},
					{0,0,2,0,0,2,0,0},
					{0,0,1,0,0,1,0,0},
					{0,0,0,0,0,0,0,0},
					{0,2,0,0,0,0,2,0},
					{0,0,2,2,2,2,0,0},
					{0,0,0,0,0,0,0,0}
				  };

void Setup(void);
unsigned char ScanKeypad(void);
unsigned char ScanColumns(unsigned char);
void DisplayKeyValue(unsigned char);
void DebounceDelay(void);

void ClearMatrix(void);
void UpdateMatrix(int, int,int);
void ToggleState(void);
void DisplayLED(void);
void Refresh(void);

void USART0_TX_SingleByte(unsigned char);
void USART0_TX_String(char*);
int atoi(char *);
void MessageProcessed(void);

void ClearMatrix()
{
	for (int i = 0; i < 8; i++)
	{
	    for (int j = 0; j < 8; j++)
	    {
			data[i][j] = 0;
		}	
	}
}

void UpdateMatrix(int keypad_x, int keypad_y, int value)
{
	//convert from 4x4 keypad to 8x8 leds, so scale 1 input to 2 leds
	int x = keypad_x *2;
	int y = keypad_y *2;
	
	for (int i = x; i < x+2; i++)
	{
		for (int j = y; j < y+2; j++)
		{
			switch (value)
			{
				case 0:
				data[i][j] = 0;
				break;
				case 2:
				data[i][j] = value;
				break;
				case 1:
				if ((i == x && j==y) || (i == x+1 && j==y+1))
				{
					data[i][j] = value;
				}
				else
				{
					data[i][j] = 0;
				}
				break;
			}
		}
	}
}

void Setup()
{
	DDRH = 0xFF; //OUTPUT RGB LED, Data Direction for Port H is set to logical 1 (see page 68)
	DDRA = 0xFF; //OUTPUT ROWS LEDS , Data Direction for Port A is set to logical 1 (see page 68)
    DDRC = 0xFF; //OUTPUT COLS LEDS, Data Direction for Port C is set to logical 1 (see page 68)
    
	DDRL = 0b11110000;	// Port L data direction register (row pins output, column pins input)
	PORTL= 0b00001111;	// Set pullups on column pins (so they read '1' when no key is pressed)
	
	PORTH = LED_RED;	//Set all bits of port H to bitcode LED_RED)
	
	useExternalMatrix = FALSE;	
	
	for (int i=0;i<8;i++)
	{
		cbi(PORTA, i); //ROWS are default 0;
		sbi(PORTC, i); //COLS are default 1;
	}
	
	ClearMatrix();
	
	//setup serial connection assuming 9600 Baud rate and 1Mhz clock
	
	//UCSR0A – USART Control and Status Register A
	// bit 7 RXC Receive Complete (flag)
	// bit 6 TXC Transmit Complete (flag)
	// bit 5 UDRE Data Register Empty (flag)
	// bit 4 FE Frame Error (flag) - programmatically clear this when writing to UCSRA
	// bit 3 DOR Data OverRun (flag)
	// bit 2 PE Parity Error
	// bit 1 UX2 Double the USART TX speed (but also depends on value loaded into the Baud Rate Registers)
	// bit 0 MPCM Multi-Processor Communication Mode
	UCSR0A = 0b00000010; // Set U2X (Double the USART Tx speed, to reduce clocking error)

// UCSR0B - USART Control and Status Register B
	// bit 7 RXCIE Receive Complete Interrupt Enable
	// bit 6 TXCIE Transmit Complete Interrupt Enable
	// bit 5 UDRIE Data Register Empty Interrupt Enable
	// bit 4 RXEN Receiver Enable
	// bit 3 TXEN Transmitter Enable
	// bit 2 UCSZ2 Character Size (see also UCSZ1:0 in UCSRC)
	// 0 = 5,6,7 or 8-bit data
	// 1 = 9-bit data
	// bit 1 RXB8 RX Data bit 8 (only for 9-bit data)
	// bit 0 TXB8 TX Data bit 8 (only for 9-bit data)
	UCSR0B = 0b10011000;  // RX Complete Int Enable, RX Enable, TX Enable, 8-bit data

// UCSR0C - USART Control and Status Register C
// *** This register shares the same I/O location as UBRRH ***
	// Bits 7:6 – UMSELn1:0 USART Mode Select (00 = Asynchronous)
	// bit 5:4 UPM1:0 Parity Mode
	// 00 Disabled
	// 10 Even parity
	// 11 Odd parity
	// bit 3 USBS Stop Bit Select
	// 0 = 1 stop bit
	// 1 = 2 stop bits
	// bit 2:1 UCSZ1:0 Character Size (see also UCSZ2 in UCSRB)
	// 00 = 5-bit data (UCSZ2 = 0)
	// 01 = 6-bit data (UCSZ2 = 0)
	// 10 = 7-bit data (UCSZ2 = 0)
	// 11 = 8-bit data (UCSZ2 = 0)
	// 11 = 9-bit data (UCSZ2 = 1)
	// bit 0 UCPOL Clock POLarity
	// 0 Rising XCK edge
	// 1 Falling XCK edge
	UCSR0C = 0b00000111;		// Asynchronous, No Parity, 1 stop, 8-bit data, Falling XCK edge

// UBRR0 - USART Baud Rate Register (16-bit register, comprising UBRR0H and UBRR0L)
	UBRR0H = 0; // 9600 baud, UBRR = 12, and  U2X must be set to '1' in UCSRA
	UBRR0L = 12;
	
	sei();
}

void ToggleState(){
	ClearMatrix();
	msgCompleted = FALSE;

	if (useExternalMatrix == FALSE)
	{
		useExternalMatrix = TRUE;
		USART0_TX_String("reset");
	}
	else
	{
		useExternalMatrix = FALSE;
	}
}

void DisplayLED(){
	if (useExternalMatrix == TRUE)
	{
		PORTH = LED_GREEN;
	}
	else
	{
		PORTH = LED_RED;
	}
}

void DebounceDelay()
{
	for(int i = 0; i < 50; i++)
	{
		for(int j = 0; j < 255; j++);
	}
}

unsigned char ScanColumns(unsigned char RowWeight)
{
	// Read bits 7,6,5,4,3 as high, as only interested in any low values in bits 2,1,0
	unsigned char ColumnPinsValue; 
	ColumnPinsValue = PINL | KeypadMaskColumns; // '0' in any column position means key pressed
	ColumnPinsValue = ~ColumnPinsValue;			// '1' in any column position means key pressed

	if(KeypadMaskColumn0 == (ColumnPinsValue & KeypadMaskColumn0))
	{
		return RowWeight + 1;	// Indicates current row + column 0
	}
	
	if(KeypadMaskColumn1 == (ColumnPinsValue & KeypadMaskColumn1))
	{
		return RowWeight + 2;	// Indicates current row + column 1
	}

	if(KeypadMaskColumn2 == (ColumnPinsValue & KeypadMaskColumn2))
	{
		return RowWeight + 3;	// Indicates current row + column 2
	}
	
	if(KeypadMaskColumn3 == (ColumnPinsValue & KeypadMaskColumn3))
	{
		return RowWeight + 4;	// Indicates current row + column 2
	}
	
	return NoKey;	// Indicate no key was pressed
}

void DisplayKeyValue(unsigned char KeyValue)
{
	if (useExternalMatrix == FALSE)
	{
		//convert from decimal to x,y
		int y = (KeyValue-1) % 4;
		int x = (KeyValue - y) / 4;
	
		//retrieve current value and add 1. If > 2, reset to 0
		int value = data[x*2][y*2] +1;
		value = value %3;
	
		UpdateMatrix(x,y,value);
	}
	else
	{
		//Convert unsigned char single digit numeric to ASCII char
		char msg[2];
	
		msg[0] = KeyValue / 10 + 0x30;
		msg[1] = KeyValue % 10 + 0x30;
		msg[2] = '\0';
		
		//Send to PC
		USART0_TX_String(msg);
	}
}

unsigned char ScanKeypad()
{
	unsigned char RowWeight;
	unsigned char KeyValue;

// ScanRow0					// Row 0 is connected to port bit 4
	RowWeight = 0x00;		// Remember which row is being scanned
	PORTL = ScanKeypadRow0;	// Set bit 7 low (Row 0), bits 6,5,4 high (rows 1,2,3)
	KeyValue = ScanColumns(RowWeight);	
	if(NoKey != KeyValue)
	{
		return KeyValue;
	}
	
// ScanRow1					// Row 1 is connected to port bit 5
	RowWeight = 0x04;		// Remember which row is being scanned
	PORTL = ScanKeypadRow1;	// Set bit 5 low (Row 1), bits 7,5,4 high (rows 0,2,3)
	KeyValue = ScanColumns(RowWeight);	
	if(NoKey != KeyValue)
	{
		return KeyValue;
	}

// ScanRow2					// Row 2 is connected to port bit 6
	RowWeight = 0x08;		// Remember which row is being scanned
	PORTL = ScanKeypadRow2;	// Set bit 4 low (Row 2), bits 7,6,3 high (rows 0,1,3)
	KeyValue = ScanColumns(RowWeight);	
	if(NoKey != KeyValue)
	{
		return KeyValue;
	}

// ScanRow3					// Row 3 is connected to port bit 7
	RowWeight = 0x0C;		// Remember which row is being scanned
	PORTL = ScanKeypadRow3;	// Set bit 3 low (Row 3), bits 7,6,5 high (rows 0,1,2)
	KeyValue = ScanColumns(RowWeight);	
	return KeyValue;
}

ISR(USART0_RX_vect) // (USART_RX_Complete_Handler) USART Receive-Complete Interrupt Handler
{
	char cData = UDR0;
	
	switch(cData)
	{
		case SS:
			msgCaret = 0;
		break;
		case ES:
			msgCompleted = TRUE;
		break;
		default:
			inboundMsg[msgCaret] = cData;
			msgCaret++;
		break;
	}
}

void USART0_TX_SingleByte(unsigned char cByte)
{
	while(!(UCSR0A & (1 << UDRE0)));	// Wait for Tx Buffer to become empty (check UDRE flag)
	UDR0 = cByte;	// Writing to the UDR transmit buffer causes the byte to be transmitted
}

void USART0_TX_String(char* sData)
{
	int iCount;
	int iStrlen = strlen(sData);
	if(0 != iStrlen)
	{
		for(iCount = 0; iCount < iStrlen; iCount++)
		{
			USART0_TX_SingleByte(sData[iCount]);
		}
		USART0_TX_SingleByte(CR);
		USART0_TX_SingleByte(LF);
	}
}

void Refresh() {
	
		//check if we should enable LED's that are shown at reduced brightness (for the middle state)
		if (frameId % reducedBrightness == 0) 
		{
			showPixel = TRUE;
		}
		else
		{
			showPixel = FALSE;
		}
		
		//do column scanning	
		for (int i=0;i<8;i++)
		{
			//set the drain to ROWS (port A)
			if ( i==0) //we lag 1 cycle to provide enough time for the LEDs to charge and drain 
			{
				cbi(PORTA,7);
			}
			else
			{
				cbi(PORTA,i-1);
			}
			
			//set the power to COLS (port C)
			for (int j=0;j<8;j++)
			{
				switch (data[i][j])
				{
				case 0: 
					sbi(PORTC,j);
					break;
				case 1:
					 if (showPixel == TRUE) {
						cbi(PORTC, j);
					 }
					 else
					 {
						 sbi(PORTC,j);
					 }
					 break;
				case 2: 
						cbi(PORTC, j);
					break;
				} 
			}
		
			sbi(PORTA,i);
			//_delay_ms(1);
			_delay_us(250);
			//DebounceDelay();			
		}
		
		frameId++;
}

void MessageProcessed(){

	msgCompleted = FALSE;

	for (int i = 0; i < sizeof(inboundMsg); i++)
	{
		inboundMsg[i] = 0;	
	}
}

int main(void)
{
	unsigned char KeyValue;
	
	Setup();
	
    while (1) 
    {
		//scan for inbound messages ready for parsing
		if(msgCompleted == TRUE && useExternalMatrix == TRUE)
		{
			int messageType = -1;
			int id = -1;
			int value = -1;
			char * strtokIndx; // this is used by strtok() as an index, see http://www.cplusplus.com/reference/cstring/strtok/
			strtokIndx = strtok(inboundMsg,",");
			
			while (strtokIndx != NULL)
			{
				if (messageType == -1)
				{
					messageType = atoi(strtokIndx);
					strtokIndx = strtok(NULL,",");     
				}
				else
				{
					if (id == -1)
					{
						id = atoi(strtokIndx);
						strtokIndx = strtok(NULL,",");     
					}
					else
					{
						value = atoi(strtokIndx);
						strtokIndx = NULL;
					}
				}
			}
			
			//convert id into x,y
			int y = (id-1) % 4;
			int x = (id - y) / 4;
			
			UpdateMatrix(x,y,value);
			
			MessageProcessed();			
		}
		
		
		//scan for button press, interupt might be better
		if(PINE & 0b00010000) 
		{
			ToggleState();
			DisplayLED();
			DebounceDelay(); //cant we fix this in hardware with capacitor?
		}
		
		//scan for 4x4 keypad press
		KeyValue = ScanKeypad();
		
		if(NoKey != KeyValue)
		{
			DisplayKeyValue(KeyValue);	// Display special chars in different format
			DebounceDelay();
		}
		
		//display 8x8 LED matrix
		Refresh();	            
    }
}