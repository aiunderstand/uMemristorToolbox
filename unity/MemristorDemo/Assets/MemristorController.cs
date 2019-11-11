using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using WaveFormsSDK;
using static ExperimentManager;

//this class uses API from waveform4j https://www.mvndoc.com/c/org.knowm/waveforms4j/org/knowm/waveforms4j/DWF.html
//https://github.com/knowm/waveforms4j/blob/e49747b722803ffd768a9cf990f6bdcb0e347b54/src/main/java/org/knowm/waveforms4j/DWF.java

//it is a mixture of:
//DWFproxy https://github.com/knowm/memristor-discovery/blob/develop/src/main/java/org/knowm/memristor/discovery/DWFProxy.java
//java port of DWF https://github.com/knowm/waveforms4j/blob/e49747b722803ffd768a9cf990f6bdcb0e347b54/src/main/java/org/knowm/waveforms4j/DWF.java
//pulse experiment https://github.com/knowm/memristor-discovery/blob/develop/src/main/java/org/knowm/memristor/discovery/gui/mvc/experiments/pulse/PulseExperiment.java
//pulse utility https://github.com/knowm/memristor-discovery/blob/develop/src/main/java/org/knowm/memristor/discovery/core/experiment_common/PulseUtility.java
//checkerboard experiment https://github.com/knowm/memristor-discovery/blob/180d3af1928f949d7085d12a6bd475a1a50de1ef/src/main/java/org/knowm/memristor/discovery/gui/mvc/experiments/boardcheck/BoardCheckExperiment.java

//We chose to port the checkerboard experiment, discrete chip test using the pulse utility class. 
//This is NOT using the Jspice component which is used in the pulse experiment class and will provide better resistance values with higher series resistors


//startAnalogCaptureBothChannelsTriggerOnWaveformGenerator
//startCustomPulseTrain

public class MemristorController
{
    public static ConcurrentQueue<string> Output = new ConcurrentQueue<string>();
    public static int hdwf;
    public static uint digitalIOStates;
    public const uint SWITCHES_MASK = 0b1111_1111_1111_1111; //enable 16 memristors
    public const uint ALL_DIO_OFF = 0b0000_0000_0000_0000; //deselect 

    //added from checkerboard experiment class
    private static float V_READ = .1f;
    private static float V_WRITE = 1f; //2
    //private static float V_WRITE1 = 0.5f; //1
    private static float V_RESET = -2f; //0
    //private static float MIN_DEVIATION = .03F; // Line trace resistance, AD2 Calibration. [AIU] NOT USED?
    private static int PULSE_WIDTH_IN_MICRO_SECONDS = 50_000;
    private static float MIN_Q = 2; // minimum ratio between erase/write resistance
    private static float MEMINLINE_MIN_R = 10; // if all states are below this (kiloohms), its stuck low
    private static float MEMINLINE_MAX_R = 100; // if all state are above this (kilohms), its stuck low
    private static float MEMINLINE_MIN_SWITCH_OFF = 1000; // if switch is below this resistance (kOhm) when OFF then its a bad switch
    private static int meminline_numFailed = 0;
    private static int COL_WIDTH = 10;
    public static float MIN_VOLTAGE_MEASURE_AMPLITUDE = .005f;
    public static int SERIES_RESISTANCE = 10000; //ohm, series resistor, so 2 x 5k ohm

    //REFACTOR model with shared properties. Refactor so that shared properties from pulseutility, this class are in model class
    public static MemristorModel Model = new MemristorModel(); 

    public enum Waveform
    {
        Square,
        HalfSine,
        TriangleUpDown,
    }

    public static void Init()
    {
        hdwf = Dwf.DeviceOpen(-1);
        
        // ///////////////////////////////////////////////////////////
        // Digital I/O //////////////////////////////////////////////
        // ///////////////////////////////////////////////////////////
        Dwf.DigitalIOOutputEnableSet(hdwf, SWITCHES_MASK);
        Dwf.DigitalIOOutputSet(hdwf, ALL_DIO_OFF);
        Dwf.DigitalIOConfigure(hdwf);
        digitalIOStates = Dwf.GetDigitalIOStatus(hdwf);
        //Debug.Log("start: " + digitalIOStates);

        // ///////////////////////////////////////////////////////////
        // Analog I/O //////////////////////////////////////////////
        // ///////////////////////////////////////////////////////////
        Dwf.SetPowerSupply(hdwf, (int) CHANNELS.CHANNEL_1, 5.0); //positive power channel
        Dwf.SetPowerSupply(hdwf, (int) CHANNELS.CHANNEL_2, -5.0); //negative power channel

        // ///////////////////////////////////////////////////////////
        // Analog Out (waveform) /////////////////////////////////////
        // ///////////////////////////////////////////////////////////
        // set analog out offset to zero, as it seems like it's not quite there by default
        Dwf.AnalogOutNodeOffsetSet(hdwf, (int) CHANNELS.CHANNEL_1, ANALOGOUTNODE.Carrier, 0);
        Dwf.AnalogOutNodeOffsetSet(hdwf, (int) CHANNELS.CHANNEL_2, ANALOGOUTNODE.Carrier, 0);
        
        // ///////////////////////////////////////////////////////////
        // Analog In (oscilloscope) //////////////////////////////////
        // ///////////////////////////////////////////////////////////
        Dwf.AnalogInChannelEnableSet(hdwf, (int) CHANNELS.CHANNEL_1, true);
        Dwf.AnalogInChannelRangeSet(hdwf, (int) CHANNELS.CHANNEL_1, 2.5);
        Dwf.AnalogInChannelEnableSet(hdwf, (int) CHANNELS.CHANNEL_2, true);
        Dwf.AnalogInChannelRangeSet(hdwf,(int) CHANNELS.CHANNEL_2, 2.5);

        // Set this to false (default=true). Need to call FDwfAnalogOutConfigure(true),
        // FDwfAnalogInConfigure(true) in order for *Set* methods to take effect.
        Dwf.DeviceAutoConfigureSet(hdwf, false);
    }

    public static String VerifyMemInlineReads(float[][] reads)
    {

        meminline_numFailed = 0;
        StringBuilder b = new StringBuilder();
        for (int i = 0; i < reads[0].Length; i++)
        {
            String testResult = "✓";
            if (i == 0)
            { // this is all switches off.
                if (reads[0][0] < MEMINLINE_MIN_SWITCH_OFF)
                { // should be in high resistance state.
                    testResult = "SWITCHES FAILED!";
                    AppendWhiteSpace(testResult, b, COL_WIDTH + 1);
                    break;
                }
            }
            else
            { // memristors

                float q1 = reads[0][i] / reads[1][i];
                float q2 = reads[2][i] / reads[1][i];

                if (reads[0][i] < MEMINLINE_MIN_R && reads[1][i] < MEMINLINE_MIN_R && reads[2][i] < MEMINLINE_MIN_R)
                {
                    testResult = "X [STK LOW]";
                    meminline_numFailed++;
                }
                else if (reads[0][i] > MEMINLINE_MAX_R && reads[1][i] > MEMINLINE_MAX_R && reads[2][i] > MEMINLINE_MAX_R)
                {
                    testResult = "X [STK HIGH]";
                    meminline_numFailed++;
                }
                else if (q1 < MIN_Q)
                {
                    testResult = "X [Q2<MIN]";
                    meminline_numFailed++;
                }
                else if (q2 < MIN_Q)
                {
                    testResult = "X [Q2<MIN]";
                    meminline_numFailed++;
                }
            }

            AppendWhiteSpace(testResult, b, COL_WIDTH + 1);
        }
        return b.ToString();
    }

    public static String FormatResistanceArray(String prefix, float[] r)
    {

        StringBuilder b = new StringBuilder();
        b.Append(prefix);
        for (int i = 0; i < r.Length; i++)
        {

            String s;
            if (r[i] > 0)
            {
                s = r[i].ToString("0.00 kΩ");
            }
            else
            {
                s = "∞ Ω";
            }

            AppendWhiteSpace(s, b, COL_WIDTH);


            b.Append("|");
        }
        return b.ToString();
    }

    public static void AppendWhiteSpace(String s, StringBuilder b, int COL_WIDTH)
    {
        // white space
        b.Append(s);
        for (int j = 0; j < (COL_WIDTH - s.Length); j++)
        {
            b.Append(" ");
        }
    }

    public static void CheckerboardExperiment() {
        var date = DateTime.Today.ToShortDateString();
        var time = DateTime.Now.ToShortTimeString();

        //HEADER
        var experiment = "Checkerboard experiment";
        var settings = string.Format("DATE: {0}  TIME: {5} V_WRITE: {1}v  V_RESET: {2}v  V_READ {3}v  SERIES_RES {4}Ω", date, V_WRITE, V_RESET, V_READ, SERIES_RESISTANCE, time);
        Logger.dataQueue.Add(experiment);
        Logger.dataQueue.Add(settings);

        int n = 17;

        StringBuilder b = new StringBuilder();
        b.Append("            ");
        for (int i = 0; i < n; i++)
        {
            String w = "ALL OFF";
            if (i > 0)
            {
                w = "S" + i;
            }
            AppendWhiteSpace(w, b, COL_WIDTH + 1);
        }

        //LOGIC
        float[][] reads = null;

        // form device
        reads = PulseUtility.TestMeminline(Waveform.HalfSine, -V_WRITE, -V_RESET, -V_READ, PULSE_WIDTH_IN_MICRO_SECONDS, PULSE_WIDTH_IN_MICRO_SECONDS, PULSE_WIDTH_IN_MICRO_SECONDS);
        Logger.dataQueue.Add(FormatResistanceArray("ERASE       ", reads[0]));
        Logger.dataQueue.Add(FormatResistanceArray("WRITE       ", reads[1]));
        Logger.dataQueue.Add(FormatResistanceArray("ERASE      ", reads[2]));
        Logger.dataQueue.Add("RESULT      " + VerifyMemInlineReads(reads));

        
        bool pulseCaptureFail = false;
        for (int i = 0; i < reads.Length; i++)
        {
            for (int j = 0; j < reads[i].Length; j++)
            {
                if (reads[i][j] == float.NaN)
                {
                    pulseCaptureFail = true;
                }
            }
        }
        if (pulseCaptureFail)
        {
            Logger.dataQueue.Add("PULSE CAPTURE FAILURE");            
        }

        if (meminline_numFailed == 0)
        {
            Logger.dataQueue.Add("TIER 1");            
        }
        else if (meminline_numFailed <= 2)
        {
            Logger.dataQueue.Add("TIER 2");
        }
        else if (meminline_numFailed <= 4)
        {
            Logger.dataQueue.Add("BURN AND LEARN");
        }
        else
        {
            Logger.dataQueue.Add("REJECT");
        }

        if (meminline_numFailed > 3)
            Logger.dataQueue.Add("NOTE: Is board in mode 2 ? ");
        


        Debug.Log("NOTE: V2 board must be in Mode 1");
        Logger.SaveExperimentDataToLog();
    }

    public static void ReadTestExperiment()
    {
        var date = DateTime.Today.ToShortDateString();
        var time = DateTime.Now.ToShortTimeString();

        //HEADER
        var experiment = "Read test experiment";
        var settings = string.Format("DATE: {0}  TIME: {5} V_WRITE: {1}v  V_RESET: {2}v  V_READ {3}v  SERIES_RES {4}Ω", date, V_WRITE, V_RESET, V_READ, SERIES_RESISTANCE, time);
        Logger.dataQueue.Add(experiment);
        Logger.dataQueue.Add(settings);
        
        // form device        
        PulseUtility.ReadExperiment(Waveform.Square, Waveform.HalfSine, -V_WRITE, -V_RESET, -V_READ, PULSE_WIDTH_IN_MICRO_SECONDS, PULSE_WIDTH_IN_MICRO_SECONDS, PULSE_WIDTH_IN_MICRO_SECONDS);

        Logger.SaveExperimentDataToLog();
    }

    public static void ReadTestAfterDisconnectExperiment()
    {
        var date = DateTime.Today.ToShortDateString();
        var time = DateTime.Now.ToShortTimeString();

        //HEADER
        var experiment = "Read test after disconnect experiment";
        var settings = string.Format("DATE: {0}  TIME: {5} V_WRITE: {1}v  V_RESET: {2}v  V_READ {3}v  SERIES_RES {4}Ω", date, V_WRITE, V_RESET, V_READ, SERIES_RESISTANCE, time);
        Logger.dataQueue.Add(experiment);
        Logger.dataQueue.Add(settings);

        // form device        
        PulseUtility.ReadAfterDisconnectExperiment(Waveform.Square, Waveform.HalfSine, -V_WRITE, -V_RESET, -V_READ, PULSE_WIDTH_IN_MICRO_SECONDS, PULSE_WIDTH_IN_MICRO_SECONDS, PULSE_WIDTH_IN_MICRO_SECONDS);

        Logger.SaveExperimentDataToLog();
    }

    public static void DCExperiment()
    {
        var date = DateTime.Today.ToShortDateString();
        var time = DateTime.Now.ToShortTimeString();

        //HEADER
        var experiment = "DC Experiment";
        var settings = string.Format("DATE: {0}  TIME: {1} SERIES_RES {2}Ω", date, time, SERIES_RESISTANCE);
        Logger.dataQueue.Add(experiment);
        Logger.dataQueue.Add(settings);

        // form device (CURRENTLY ARGUMENTS ARE NOT USED, REFACTOR)   
        //do all 16 memristors
        for (int i = 0; i < 16; i++)
        {
            MemristorController.ToggleMemristor(i, true);
            PulseUtility.DCExperiment(i, Waveform.Square, Waveform.HalfSine, -V_WRITE, -V_RESET, -V_READ, PULSE_WIDTH_IN_MICRO_SECONDS, PULSE_WIDTH_IN_MICRO_SECONDS, PULSE_WIDTH_IN_MICRO_SECONDS);
            MemristorController.ToggleMemristor(i, false);
        }

        Logger.SaveExperimentDataToLog();
    }

    public static void OneTritADCExperiment()
    {
        throw new NotImplementedException();
    }

    public static void Read(int id)
    {
        //do actual read



        //put result in Output queue


    }

    public static void Send(int id, string message)
    {
        //check with output if needing change before sending



    }

    public static void Close()
    {
        //inspired by java port by Knowm https://github.com/knowm/memristor-discovery/blob/develop/src/main/java/org/knowm/memristor/discovery/DWFProxy.java

        // ///////////////////////////////////////////////////////////
        // Digital I/O //////////////////////////////////////////////
        // ///////////////////////////////////////////////////////////
        Dwf.DigitalIOReset(hdwf);
        Dwf.DigitalOutReset(hdwf);

        // ///////////////////////////////////////////////////////////
        // Analog Out ///////////////////////////////////////////////
        // ///////////////////////////////////////////////////////////
        Dwf.AnalogOutConfigure(hdwf, (int)CHANNELS.CHANNEL_BOTH, false);

        // ///////////////////////////////////////////////////////////
        // Analog In ///////////////////////////////////////////////
        // ///////////////////////////////////////////////////////////
        Dwf.AnalogInConfigure(hdwf,false, false);

        // ///////////////////////////////////////////////////////////
        // Analog I/O //////////////////////////////////////////////
        // ///////////////////////////////////////////////////////////
        Dwf.SetPowerSupply(hdwf, (int) CHANNELS.CHANNEL_1, 0.0);
        Dwf.SetPowerSupply(hdwf, (int) CHANNELS.CHANNEL_2, 0.0);
        
        // ///////////////////////////////////////////////////////////
        // Device //////////////////////////////////////////////
        // ///////////////////////////////////////////////////////////
        Dwf.DeviceCloseAll();
    }

    public static void StopWave(int idxChannel)
    {
        Dwf.AnalogOutNodeOffsetSet(hdwf, idxChannel, ANALOGOUTNODE.Carrier,  0); // shouldn't need this in theory, but DC offset is always lingering (https://forum.digilentinc.com/topic/3465-waveforms-sdk-correctly-start-and-stop-analog-out/)
        Dwf.AnalogOutConfigure(hdwf,idxChannel, false);
    }

    public static void StopAnalogCaptureBothChannels()
    {
        Dwf.AnalogInConfigure(hdwf,true, false);
    }

    public static void TurnOffAllSwitches()
    {
        //var mask = Convert.ToUInt32("0000000000000000", 2);
        //digitalIOStates = digitalIOStates & mask;
        //Dwf.DigitalIOOutputSet(hdwf, digitalIOStates);
        Dwf.DigitalIOOutputSet(hdwf, ALL_DIO_OFF);

        Dwf.DigitalIOConfigure(hdwf);
        digitalIOStates = Dwf.GetDigitalIOStatus(hdwf);
    }

    public static void ToggleMemristor(int memristorID, bool isOn)
    {
        if (isOn)
        {
            digitalIOStates = digitalIOStates | (uint) (1 << memristorID);
        }
        else
        {
            digitalIOStates = digitalIOStates & (uint) ~(1 << memristorID);
        }

        //Debug.Log("before: " + digitalIOStates);
        Dwf.DigitalIOOutputSet(hdwf, digitalIOStates);
        Dwf.DigitalIOConfigure(hdwf);
        digitalIOStates = Dwf.GetDigitalIOStatus(hdwf);
        //Debug.Log("after: " + digitalIOStates);
    }

    public static void WaitUntilArmed()
    {
        //should make this into UI friendly co-routine because now it is a blocking call could potentially hang the app if not armed 
        while (true)
        {
            var status = Dwf.AnalogInStatus(hdwf, true);
            if (status == STATE.Armed) // armed
            { 
                break;
            }
        }
    }

    public static bool CapturePulseData(double frequency, int pulseNumber)
    {
        //should make this into UI friendly co-routine because now it is a blocking call waiting to have a read done or bailcount interations 
        // Read In Data
        int bailCount = 0;
        while (true)
        {
            try
            {
                var sleepTime = (int)(1 / frequency * pulseNumber * 1000);
                Thread.Sleep(sleepTime); //in milliseconds
            }
            catch (Exception e)
            {
                Debug.Log("CapturePulseData:" + e);
            }
            var status = Dwf.AnalogInStatus(hdwf, true);
           
            if (status == STATE.Done)
            { 
                return true;
            }

            if (bailCount++ > 1000)
            {
                Debug.Log("CaputurePulseData: Read time out");
                return false;
            }
        }
    }

    public static bool StartAnalogCaptureBothChannelsTriggerOnWaveformGenerator(int waveformGenerator, double sampleFrequency, int bufferSize, bool isScale2Volts)
    {
            if (bufferSize > (int) AD2.MAX_BUFFER_SIZE)
            {
                // logger.error("Buffer size larger than allowed size. Setting to " + DWF.AD2_MAX_BUFFER_SIZE);
                bufferSize = (int) AD2.MAX_BUFFER_SIZE;
            }

            Dwf.AnalogInFrequencySet(hdwf, sampleFrequency);
            Dwf.AnalogInBufferSizeSet(hdwf, bufferSize);
            Dwf.AnalogInTriggerPositionSet(hdwf, (bufferSize / 2) / sampleFrequency); // no buffer prefill

            Dwf.AnalogInChannelEnableSet(hdwf,(int)CHANNELS.CHANNEL_1, true);
            Dwf.AnalogInChannelRangeSet(hdwf, (int)CHANNELS.CHANNEL_1, isScale2Volts ? 2 : 6);
            Dwf.AnalogInChannelEnableSet(hdwf, (int)CHANNELS.CHANNEL_2, true);
            Dwf.AnalogInChannelRangeSet(hdwf, (int)CHANNELS.CHANNEL_2, isScale2Volts ? 2 : 6);

            Dwf.AnalogInAcquisitionModeSet(hdwf,ACQMODE.Single);

            // Trigger single capture on rising edge of analog signal pulse
            Dwf.AnalogInTriggerAutoTimeoutSet(hdwf,0); // disable auto trigger

            if (waveformGenerator == (int) CHANNELS.CHANNEL_1)
            {
                Dwf.AnalogInTriggerSourceSet(hdwf,TRIGSRC.AnalogOut1); // one of the analog in channels
            }
            else if (waveformGenerator == (int)CHANNELS.CHANNEL_2)
            {
                Dwf.AnalogInTriggerSourceSet(hdwf,TRIGSRC.AnalogOut2); // one of the analog in channels
            }

            Dwf.AnalogInTriggerTypeSet(hdwf, TRIGTYPE.Edge);
            Dwf.AnalogInTriggerChannelSet(hdwf,0); // first channel

            // arm the capture
            Dwf.AnalogInConfigure(hdwf,true, true);

            //NEED TO REFACTOR IF WE WANT TO HAVE THIS ERROR LOG
            //if (!success) 
            //{
            //Dwf.AnalogInChannelEnableSet(hdwf,(int)CHANNELS.CHANNEL_1, true);
            //Dwf.AnalogInChannelEnableSet(hdwf, (int)CHANNELS.CHANNEL_2, true);
            //Dwf.AnalogInConfigure(hdwf, false, false);
            //Debug.LogError("Error in analog capture triggered by waveformgenerator");
            //}
            return true;
        }

    public static bool StartAnalogCaptureBothChannelsLevelTrigger(double sampleFrequency, double triggerLevel, int bufferSize)
    {
        if (bufferSize > (int)AD2.MAX_BUFFER_SIZE)
        {
            // logger.error("Buffer size larger than allowed size. Setting to " + DWF.AD2_MAX_BUFFER_SIZE);
            bufferSize = (int)AD2.MAX_BUFFER_SIZE;
        }
        
        Dwf.AnalogInFrequencySet(hdwf, sampleFrequency);
        Dwf.AnalogInBufferSizeSet(hdwf, bufferSize);
        Dwf.AnalogInTriggerPositionSet(hdwf, (bufferSize / 2) / sampleFrequency); // no buffer prefill

        Dwf.AnalogInChannelEnableSet(hdwf, (int)CHANNELS.CHANNEL_1, true);
        Dwf.AnalogInChannelRangeSet(hdwf, (int)CHANNELS.CHANNEL_1, 2.5);
        Dwf.AnalogInChannelEnableSet(hdwf, (int)CHANNELS.CHANNEL_2, true);
        Dwf.AnalogInChannelRangeSet(hdwf, (int)CHANNELS.CHANNEL_2, 2.5);

        Dwf.AnalogInAcquisitionModeSet(hdwf, ACQMODE.Single);

        // Trigger single capture on rising edge of analog signal pulse
        Dwf.AnalogInTriggerAutoTimeoutSet(hdwf, 0); // disable auto trigger
        
        Dwf.AnalogInTriggerSourceSet(hdwf, TRIGSRC.DetectorAnalogIn); // one of the analog in channels

        Dwf.AnalogInTriggerTypeSet(hdwf, TRIGTYPE.Edge);
        Dwf.AnalogInTriggerChannelSet(hdwf, 0); // first channel
                                                             
        // Trigger Level
        if (triggerLevel > 0)
        {
            Dwf.AnalogInTriggerConditionSet(hdwf, TRIGCOND.RisingPositive);
            Dwf.AnalogInTriggerLevelSet(hdwf,triggerLevel);
        }
        else
        {
            Dwf.AnalogInTriggerConditionSet(hdwf, TRIGCOND.FallingNegative);
            Dwf.AnalogInTriggerLevelSet(hdwf, triggerLevel);
        }

        // arm the capture
        Dwf.AnalogInConfigure(hdwf, true, true);

        //NEED TO REFACTOR IF WE WANT TO HAVE THIS ERROR LOG
        //if (!success) 
        //{
        //Dwf.AnalogInChannelEnableSet(hdwf,(int)CHANNELS.CHANNEL_1, true);
        //Dwf.AnalogInChannelEnableSet(hdwf, (int)CHANNELS.CHANNEL_2, true);
        //Dwf.AnalogInConfigure(hdwf, false, false);
        //Debug.LogError("Error in analog capture triggered by waveformgenerator");
        //}

        return true;
    }

    public static bool StartCustomPulseTrain(int idxChannel, double frequency, double offset, int numPulses, double[] rgdData)
    {
        Dwf.AnalogOutRepeatSet(hdwf, idxChannel, 1);
        double secRun = 1 / frequency * numPulses;
        Dwf.AnalogOutRunSet(hdwf, idxChannel, secRun);
        //Dwf.AnalogOutIdleSet(hdwf, idxChannel, AnalogOutIdle.Offset); // when idle, what's the DC level? answer: the offset level, disabled in original code
        Dwf.AnalogOutNodeEnableSet(hdwf, idxChannel, ANALOGOUTNODE.Carrier ,true); //guessed to be carrier
        Dwf.AnalogOutNodeFunctionSet(hdwf, idxChannel,ANALOGOUTNODE.Carrier,FUNC.Custom); //guessed to be carrier
        Dwf.AnalogOutNodeFrequencySet(hdwf, idxChannel, ANALOGOUTNODE.Carrier, frequency); //guessed to be carrier
        Dwf.AnalogOutNodeAmplitudeSet(hdwf, idxChannel, ANALOGOUTNODE.Carrier, 5.0); // manually set to full amplitude //guessed to be carrier
        Dwf.AnalogOutNodeOffsetSet(hdwf, idxChannel, ANALOGOUTNODE.Carrier, offset); //guessed to be carrier
        Dwf.AnalogOutNodeDataSet(hdwf, idxChannel, ANALOGOUTNODE.Carrier, rgdData); //guessed to be carrier
        Dwf.AnalogOutConfigure(hdwf, idxChannel, true);

        //NEED TO REFACTOR IF WE WANT TO HAVE THIS ERROR LOG
        //if (!success)
        //{
        //    FDwfAnalogOutNodeEnableSet(idxChannel, false);
        //    FDwfAnalogOutConfigure(idxChannel, false);
        //    throw new DWFException(FDwfGetLastErrorMsg());
        //}
        return true;
    }

}
