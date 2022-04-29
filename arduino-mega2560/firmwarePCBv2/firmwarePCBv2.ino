/*
 Memristor Development Platform firmware  
 created 21 April 2022
 by Steven Bos, Mehtab Singh Virk, Henning Gundersen 
 
 Changelog
 v0.1 - initial firmware for MVP 
 v0.2 - added serial command interface:
          - p-1 is reverse polarity (NOT FULLY IMPLEMENTED WITH PULSE TRAIN), p0 is disconnected, p1 is forward polarity
          - q0 is power off, q1 is power on of -5 and +5 voltage rails
          - w1 is fire one pulse of 62.5ns. w2 fires 2 pulses of 62.5ns etc. It is possible to increase the pulse width via code but not via serial interface
          - m1 is set mux to 1 thus selecting memristor 1. m2 selects memristor 2 etc.
      - integrated digitpot X9258 (see https://www.renesas.com/us/en/document/dst/x9258-datasheet) 
          - i2c read from wiper counter register of digipot 1-4
          - i2c write to wiper counter register of digipot 1-4
          - i2c non-volatile write to wiper counter register of digipot 1-4
      - integrated 8:1 multiplexer 
      - integrated "fake memristor", which can be set via code to any value between 0 Ohm (+ terminal resistance of 40 Ohm) to 100k Ohm in steps of 390.625 Ohm using 8 bits
      - integrated one-shot pulse train with variable pulse width and pulse amount.
       
IDEAS for new versions:
 + add another OCR for reverse polarity and enable switching between them (reverse and forward pulse train are not allowed to be active at the same time!) 
 + simplify instructions for use with uMemristorToolbox, eg. 
      -WriteStateFast(00) = state 0 using an erase pulse first and then a pulse width, amount sequence to reach state WITHOUT checking if actually reached
      -WriteStatePrecise(22) = state 8 using a read pulse first and then send a delta pulse, read pulse next, send a delta pulse until reached state (or fail after x attempts).   
         - this function is needed for a nice multivalued pyramid plot.
 + integrate with uMemristorToolbox using serial communication
 + Is pulse+ and pulse- pulled down at bootup, thus disconnected so that the memristor is not overwritten at bootup? If not we need pull down resistors?
*/


/*--------------------DECLARATIONS--------------------------*/
#include <Wire.h>
//https://www.arduino.cc/en/reference/wire
//https://www.coridium.us/cHELP/scr/HwI2c.htm
//https://docs.arduino.cc/learn/communication/wire
//https://forum.arduino.cc/t/reading-and-setting-the-i2c-address-on-an-hmc6352-compass-module/87460

int X9258Address = 0x50; //digipot slave address if ADDR0,ADDR1,ADDR2,ADDR3 are LOW

//  Shift the device's documented slave address (0x42) 1 bit right
//  This compensates for how the TWI library only wants the
//  7 most significant bits (with the high bit padded with 0)
//  eg. slaveAddress = HMC6352Address >> 1;   // This results in 0x21 as the address to pass to TWI
int dpAddr = X9258Address >> 1;  //should thus be 168 = 0b10101000

byte i2c_rcv;
void i2cWrite(byte b1, byte b2);

//pin declarations
#define  pin_enablePlus5v 2
#define pin_enableMin5v 3

#define pin_enablePlusPulse 9
#define pin_enableMinPulse 5

#define pin_muxA0 27
#define pin_muxA1 28
#define pin_muxA2 29
#define pin_muxEnable1 30
#define pin_muxEnable2 31
#define pin_muxEnable3 32
#define pin_muxEnable4 33

#define pin_dpWriteProtect 26
#define pin_dpAddr0 22
#define pin_dpAddr1 23
#define pin_dpAddr2 24
#define pin_dpAddr3 25

int pulseDir = 0; //-1 is reverse, 0 is disconnected, 1 is forward


/*--------------------FUNCTIONS--------------------------*/
void i2cWrite(byte b1, byte b2)
{
    Wire.beginTransmission(dpAddr); 
    Wire.write(b1);
    Wire.write(b2);
    Wire.endTransmission();
}

void i2cRead(byte b1)
{
    Wire.beginTransmission(dpAddr); 
    Wire.write(b1);
    Wire.endTransmission();    

    Wire.requestFrom(168, 1);  

    if(Wire.available()) {        
      i2c_rcv = Wire.read();
      Serial.print("Read: ");
      Serial.println(i2c_rcv);
    }
}

void SwitchPower(int state)
{
  if (state == 1)
  {
    digitalWrite(pin_enablePlus5v, HIGH);
    digitalWrite(pin_enableMin5v, HIGH); 
    Serial.println("Power is ON"); 
  }

  if (state == 0)
  {
    digitalWrite(pin_enablePlus5v, LOW);
    digitalWrite(pin_enableMin5v, LOW);  
    Serial.println("Power is OFF");
  }
}

void SetPulseTo(int state)
{
  if (state == -1)
  {
    digitalWrite(pin_enablePlusPulse, LOW);
    digitalWrite(pin_enableMinPulse, HIGH); 
    pulseDir = -1;
    Serial.println("Pulse set to REVERSE"); 
  }

  if (state == 0)
  {
    digitalWrite(pin_enablePlusPulse, LOW);
    digitalWrite(pin_enableMinPulse, LOW);  
    pulseDir = 0;
    Serial.println("Pulse set to DISCONNECT");
  }

  if (state == 1)
  {
    digitalWrite(pin_enablePlusPulse, HIGH);
    digitalWrite(pin_enableMinPulse, LOW);
    pulseDir = 1;
    Serial.println("Pulse set to FORWARD");  
  }

  //NOTE: NEVER SET BOTH TO HIGH!
}

void SelectMemristor (int memristor)
{
  if (memristor == 1)
  {
   digitalWrite(pin_muxA0, LOW);
   digitalWrite(pin_muxA1, LOW);
   digitalWrite(pin_muxA2, LOW);
   
   Serial.println("Memristor 1 SELECTED");
  }

  if (memristor == 2)
  {
   digitalWrite(pin_muxA0, HIGH);
   digitalWrite(pin_muxA1, LOW);
   digitalWrite(pin_muxA2, LOW);

   Serial.println("Memristor 2 SELECTED");
  }

  if (memristor == 3)
  {
   digitalWrite(pin_muxA0, LOW);
   digitalWrite(pin_muxA1, HIGH);
   digitalWrite(pin_muxA2, LOW);

   Serial.println("Memristor 3 SELECTED");
  }

  if (memristor == 4)
  {
   digitalWrite(pin_muxA0, HIGH);
   digitalWrite(pin_muxA1, HIGH);
   digitalWrite(pin_muxA2, LOW);

   Serial.println("Memristor 4 SELECTED");
  }

  if (memristor == 5)
  {
   digitalWrite(pin_muxA0, LOW);
   digitalWrite(pin_muxA1, LOW);
   digitalWrite(pin_muxA2, HIGH);

   Serial.println("Memristor 5 SELECTED");
  }

  if (memristor == 6)
  {
   digitalWrite(pin_muxA0, HIGH);
   digitalWrite(pin_muxA1, LOW);
   digitalWrite(pin_muxA2, HIGH);

   Serial.println("Memristor 6 SELECTED");
  }

  if (memristor == 7)
  {
   digitalWrite(pin_muxA0, LOW);
   digitalWrite(pin_muxA1, HIGH);
   digitalWrite(pin_muxA2, HIGH);

   Serial.println("Memristor 7 SELECTED");
  }

  if (memristor == 8)
  {
   digitalWrite(pin_muxA0, HIGH);
   digitalWrite(pin_muxA1, HIGH);
   digitalWrite(pin_muxA2, HIGH);

   Serial.println("Memristor 8 SELECTED");
  }

  //NOTE: THIS MULTIPLEXER ONLY ALLOWS 8 MEMRISTORS OF THE 16 TO BE SELECTED
}

void WriteDigipot(byte instruction, byte state)
{
  i2cWrite(instruction,state);
}

void ReadDigitpot(byte instruction)
{
  i2cRead(instruction);
}

#define OSP_SET_WIDTH(cycles) (OCR2B = 0xff-(cycles-1))

// Setup the one-shot pulse generator and initialize with a pulse width that is (cycles) clock counts long

void osp_setup(uint8_t cycles) {
  TCCR2B =  0;      // Halt counter by setting clock select bits to 0 (No clock source).
              // This keeps anyhting from happeneing while we get set up

  TCNT2 = 0x00;     // Start counting at bottom. 
  OCR2A = 0;      // Set TOP to 0. This effectively keeps us from counting becuase the counter just keeps reseting back to 0.
          // We break out of this by manually setting the TCNT higher than 0, in which case it will count all the way up to MAX and then overflow back to 0 and get locked up again.
  OSP_SET_WIDTH(cycles);    // This also makes new OCR values get loaded frm the buffer on every clock cycle. 

  TCCR2A = _BV(COM2B0) | _BV(COM2B1) | _BV(WGM20) | _BV(WGM21); // OC2B=Set on Match, clear on BOTTOM. Mode 7 Fast PWM.
  TCCR2B = _BV(WGM22)| _BV(CS20);         // Start counting now. WGM22=1 to select Fast PWM mode 7

  DDRD |= _BV(3);     // Set pin to output (Note that OC2B = GPIO port PD3 = Arduino Digital Pin 3)
}

// Setup the one-shot pulse generator
void osp_setup() {
  osp_setup(1);
}

// Fire a one-shot pulse. Use the most recently set width. 

#define OSP_FIRE() (TCNT2 = OCR2B - 1)

// Test there is currently a pulse still in progress

#define OSP_INPROGRESS() (TCNT2>0)

// Fire a one-shot pusle with the specififed width. 
// Order of operations in calculating m must avoid overflow of the unint8_t.
// TCNT2 starts one count lower than the match value becuase the chip will block any compare on the cycle after setting a TCNT. 

#define OSP_SET_AND_FIRE(cycles) {uint8_t m=0xff-(cycles-1); OCR2B=m;TCNT2 =m-1;}

void FirePulseTrain(int pulseAmount, int pulseWidth)
{
 for (int i = 0; i < pulseAmount; i++)
 {
     OSP_SET_AND_FIRE(pulseWidth);
 }
}



/*--------------------MAIN APPLICATION--------------------------*/

void setup() {
Serial.begin(9600);
Wire.begin();

//PINS
//set direction
pinMode(pin_enablePlus5v, OUTPUT);
pinMode(pin_enableMin5v, OUTPUT);
pinMode(pin_enablePlusPulse, OUTPUT);
pinMode(pin_enableMinPulse, OUTPUT);

pinMode(pin_muxA0, OUTPUT);
pinMode(pin_muxA1, OUTPUT);
pinMode(pin_muxA2, OUTPUT);
pinMode(pin_muxEnable1, OUTPUT);
pinMode(pin_muxEnable2, OUTPUT);
pinMode(pin_muxEnable3, OUTPUT);
pinMode(pin_muxEnable4, OUTPUT);

pinMode(pin_dpWriteProtect, OUTPUT);
pinMode(pin_dpAddr0, OUTPUT);
pinMode(pin_dpAddr1, OUTPUT);
pinMode(pin_dpAddr2, OUTPUT);
pinMode(pin_dpAddr3, OUTPUT);

delay(10);
Serial.println("Booted up");

//default states
SwitchPower(1); //power on

SetPulseTo(0); //disconnected from memristor

digitalWrite(pin_muxEnable1, HIGH);
digitalWrite(pin_muxEnable2, HIGH);
digitalWrite(pin_muxEnable3, HIGH);
digitalWrite(pin_muxEnable4, HIGH);

SelectMemristor(1);//select memristor 1

delay(100);// wait 100ms to stabilize.

 //DIGITPOT
//P0=0, P1=0 (digipot 0 for FakeMem, 100K) 
//P0=0, P1=1 (digipot 1 for CurrentSource FixedRC, 100K)  
//P0=1, P1=0 (digipot 2 for WaveForm, 5K )
//P0=1, P1=1 (digipot 3 for CurrentSource CurrentControl, 10.5K)

//R0=0, R1=0 (we only use 1 register)

//instructions
//I3=1, I2=0, I1=0, I0=1  (Read Counter Pot x, registers don't care)
//I3=1, I2=0, I1=1, I0=0  (Write Counter Pot x, registers don't care)
//I3=1, I2=0, I1=1, I0=1  (Read Register Pot x Register X)
//I3=1, I2=1, I1=0, I0=0  (Write Register Pot x Register X)
//I3=0, I2=0, I1=1, I0=0  (Incr./Decr. Counter Pot x, registers don't care)

//data
//I7=1 ... I7=1 (0 Ohms)
//I7=0 ... I7=0 (100 kOhms)
//256 steps, each step is 390.625 Ohm 
//5K = 13 = 1111 0010 (0xF2)
//10.5K = 27 = 1110 0100 (0xE4)

//VOLATILE SET DIGITPOT WIPER 0/EMULATED MEMRISTOR to 100K (0x00 = 100K = 255x390.625)
WriteDigipot(0xA0,0x00); // volatile (only update counter register, 0b1001 0000)

//digitalWrite(pin_dpWriteProtect, HIGH);
//WriteDigipot(0xC0,0x00); // non-volatile (update register 0 of digipot 0, 0b1100 0000)
delay(10);
//digitalWrite(pin_dpWriteProtect, LOW);

ReadDigitpot(0x90); //read wipercounterregister of digipot 0
ReadDigitpot(0xB0); //read register 0 of digitpot 0 


//SET DIGITPOT WIPER 1/FIXED CURRENTSOURCE to 100K (0x00 = 100K = 255x390.625)
WriteDigipot(0xA1,0x00);
delay(10);
ReadDigitpot(0x91);

//VOLATILE DIGITPOT WIPER 2 SENSE RESISTOR to 3.6K (0xF6 = 3.6K, 0xF2 = 5K = 13x390.625)
WriteDigipot(0xA2,0xF5); //F5 = 3.9K
delay(10);
ReadDigitpot(0x92);

//SET DIGITPOT WIPER 3/VARIABLE CURRENTSOURCE to 10.5K (0xE4 = 10.5K = 27x390.625)
WriteDigipot(0xA3,0xE4); //
delay(10);
ReadDigitpot(0x93);

//Initialize hardware timer for pulse train
//https://github.com/bigjosh/TimerShot/blob/master/TimerShot.ino
//https://wp.josh.com/2015/03/05/the-perfect-pulse-some-tricks-for-generating-precise-one-shots-on-avr8/
//https://wp.josh.com/2015/03/12/avr-timer-based-one-shot-explained/

osp_setup();
}

void loop() {
  if (Serial.available() > 0)
  {
   char property = Serial.read();
   
   switch (property) {
      case 'p':  //pulse
      {
          Serial.println(property);     
          int state = Serial.parseInt();
          Serial.println(state);     
          SetPulseTo(state);
      }
      break;
      case 'm':  //mux
      {
          Serial.println(property);     
          int state = Serial.parseInt();
          Serial.println(state);     
          SelectMemristor(state);
      }
      break;
      case 'q':  //power
      {
          Serial.println(property);     
          int state = Serial.parseInt();
          Serial.println(state);     
          SwitchPower(state);
      }
      break;
       case 'w':  //set pulse train
      {
          Serial.println(property);     
          int pulseAmount = Serial.parseInt();
          Serial.println(pulseAmount);     
          FirePulseTrain(pulseAmount,1); //pulsewidth 1 clock cycle, with 16MHz clock is 62.5ns
      }
      break;
      default:
      {
        //ignore
      }
      break;
    }
  }
}
