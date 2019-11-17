/* THIS CODE HAS BEEN PORTED AND ADAPTED FROM https://raw.githubusercontent.com/knowm/memristor-discovery/develop/src/main/java/org/knowm/memristor/discovery/core/experiment_common/PulseUtility.java
 * Author: AIUNDERSTAND  


/**
 * Memristor-Discovery is distributed under the GNU General Public License version 3 and is also
 * available under alternative licenses negotiated directly with Knowm, Inc.
 *
 * <p>Copyright (c) 2016-2019 Knowm Inc. www.knowm.org
 *
 * <p>Knowm, Inc. holds copyright and/or sufficient licenses to all components of the
 * Memristor-Discovery package, and therefore can grant, at its sole discretion, the ability for
 * companies, individuals, or organizations to create proprietary or open source (even if not GPL)
 * modules which may be dynamically linked at runtime with the portions of Memristor-Discovery which
 * fall under our copyright/license umbrella, or are distributed under more flexible licenses than
 * GPL.
 *
 * <p>The 'Knowm' name and logos are trademarks owned by Knowm, Inc.
 *
 * <p>If you have any questions regarding our licensing policy, please contact us at
 * `contact@knowm.org`.
 */

using System;
using System.Threading;
using UnityEngine;
using WaveFormsSDK;
using static MemristorController;
using static TimeUnitsHelper;
using static ConductanceUnitsHelper;
using static CurrentUnitsHelper;
using static ResistanceUnitsHelper;
using static ExperimentManager;
using TMPro;

public class PulseUtility
{
    public enum PulseType
    {
        Read,
        WriteState1,
        WriteState2,
        Erase,
        Error
    }
    //refactor all these variables in the Model. The variables below are only used in some experiments
    static int _samplesPerPulse; 
    static int _sampleFrequency;
    static int _samples; //used setting getScopesAverageVoltage
    static int _readSamples = 10;
    static int _readDelay = 1000; //miliseconds
    public static float[][] TestMeminline(Waveform writeEraseWaveform, float V_WRITE, float V_ERASE, float V_READ, int READ_PULSE_WIDTH_IN_MICRO_SECONDS, int WRITE_PULSE_WIDTH_IN_MICRO_SECONDS, int ERASE_PULSE_WIDTH_IN_MICRO_SECONDS)
    {

        try
        {
            //  initialize in erased state
            measureAllSwitchResistances(writeEraseWaveform, V_ERASE, ERASE_PULSE_WIDTH_IN_MICRO_SECONDS);
            Thread.Sleep(25);

            float[][] reads = new float[3][];

            reads[0] = measureAllSwitchResistances(Waveform.Square, V_READ, READ_PULSE_WIDTH_IN_MICRO_SECONDS);

            Thread.Sleep(25);
            measureAllSwitchResistances(writeEraseWaveform, V_WRITE, WRITE_PULSE_WIDTH_IN_MICRO_SECONDS);
            Thread.Sleep(25);
            reads[1] = measureAllSwitchResistances(Waveform.Square, V_READ, READ_PULSE_WIDTH_IN_MICRO_SECONDS);
            Thread.Sleep(25);
            measureAllSwitchResistances(writeEraseWaveform, V_ERASE, ERASE_PULSE_WIDTH_IN_MICRO_SECONDS);
            Thread.Sleep(25);
            reads[2] = measureAllSwitchResistances(Waveform.Square, V_READ, READ_PULSE_WIDTH_IN_MICRO_SECONDS);

            Logger.dataQueue.Add(string.Format("Read Waveform;{0};WriteErase Waveform;{1};Samples per pulse;{2};SampleFrequency;{3};samples{4}", Waveform.Square.ToString(), Waveform.HalfSine.ToString(), _samplesPerPulse, _sampleFrequency, _samples));

            return reads;


        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void EraseWriteReadTestExperiment(Waveform readWaveform, Waveform writeEraseWaveform, float V_WRITE, float V_ERASE, float V_READ, int READ_PULSE_WIDTH_IN_MICRO_SECONDS, int WRITE_PULSE_WIDTH_IN_MICRO_SECONDS, int ERASE_PULSE_WIDTH_IN_MICRO_SECONDS)
    {
        try
        {
            float[][] reads = new float[1][];

            //1 erase
            measureAllSwitchResistances(writeEraseWaveform, V_ERASE, ERASE_PULSE_WIDTH_IN_MICRO_SECONDS);
            Thread.Sleep(25);

            //1 write
            measureAllSwitchResistances(writeEraseWaveform, V_WRITE, WRITE_PULSE_WIDTH_IN_MICRO_SECONDS);
            Thread.Sleep(25);

            //n reads
            for (int i = 0; i < _readSamples; i++)
            {
                reads[0] = measureAllSwitchResistances(readWaveform, V_READ, READ_PULSE_WIDTH_IN_MICRO_SECONDS);
                Logger.dataQueue.Add(FormatResistanceArray("READ      ", reads[0]));
                Thread.Sleep(_readDelay);

            }
            reads[0] = measureAllSwitchResistances(readWaveform, V_READ, READ_PULSE_WIDTH_IN_MICRO_SECONDS);
            Logger.dataQueue.Add(string.Format("Read Waveform;{0};WriteErase Waveform;{1};samples per pulse;{2};sampleFrequency;{3};samples{4}", readWaveform.ToString(), writeEraseWaveform.ToString(), _samplesPerPulse, _sampleFrequency, _samples));
        }
        catch (Exception)
        {
        }
    }

    public static void ReadAfterDisconnectExperiment(Waveform readWaveform, Waveform writeEraseWaveform, float V_WRITE, float V_ERASE, float V_READ, int READ_PULSE_WIDTH_IN_MICRO_SECONDS, int WRITE_PULSE_WIDTH_IN_MICRO_SECONDS, int ERASE_PULSE_WIDTH_IN_MICRO_SECONDS)
    {
        try
        {
            float[][] reads = new float[1][];

            //n reads
            for (int i = 0; i < _readSamples; i++)
            {
                reads[0] = measureAllSwitchResistances(readWaveform, V_READ, READ_PULSE_WIDTH_IN_MICRO_SECONDS);
                Logger.dataQueue.Add(FormatResistanceArray("READ      ", reads[0]));
                Thread.Sleep(_readDelay);

            }
            reads[0] = measureAllSwitchResistances(readWaveform, V_READ, READ_PULSE_WIDTH_IN_MICRO_SECONDS);
            Logger.dataQueue.Add(string.Format("Read Waveform;{0};WriteErase Waveform;{1};Samples per pulse;{2};SampleFrequency;{3};Samples{4}", readWaveform.ToString(), writeEraseWaveform.ToString(), _samplesPerPulse, _sampleFrequency, _samples));
        }
        catch (Exception)
        {
        }
    }

    public static float[] measureAllSwitchResistances(Waveform waveform, float readVoltage, int pulseWidthInMicroSeconds)
    {
        float[] r_array = new float[17];
        //all switches off
        r_array[0] = getSwitchResistancekOhm(waveform, readVoltage, pulseWidthInMicroSeconds, (int)CHANNELS.CHANNEL_1);

        //set switch on one by one
        for (int i = 0; i < 16; i++)
        {
            MemristorController.ToggleMemristor(i, true);

            try
            {
                Thread.Sleep(5); //this is not the Unity way, blocking call!
            }
            catch (Exception)
            {

            }

            r_array[i + 1] = getSwitchResistancekOhm(waveform, readVoltage, pulseWidthInMicroSeconds, (int)CHANNELS.CHANNEL_1);

            MemristorController.ToggleMemristor(i, false);
        }

        return r_array;
    }

    public static float getSwitchResistancekOhm(Waveform waveform, float readVoltage, int pulseWidthInMicroSeconds, int dWFWaveformChannel)
    {
        float[] vMeasure = getScopesAverageVoltage(waveform, readVoltage, pulseWidthInMicroSeconds, dWFWaveformChannel);

        //Debug.Log("Measuring switch resistance. [V1,V2]=" + vMeasure);

        if (vMeasure == null)
        {
            Debug.Log("WARNING: getScopesAverageVoltage() returned null. This is likely a pulse catpure failure.");
            return float.NaN;
        }

        if (Math.Abs(vMeasure[1] - vMeasure[0]) <= MemristorController.MIN_VOLTAGE_MEASURE_AMPLITUDE)
        {
            Debug.Log("WARNING: Voltage drop across series resistor (" + vMeasure[1] + ") is at or below noise threshold.");
            return float.PositiveInfinity;
        }

        float I = (vMeasure[1] - vMeasure[0]) / MemristorController.SERIES_RESISTANCE;
        float rSwitch = Math.Abs(vMeasure[0] / I);

        return rSwitch / 1000; // to kilohms        
    }

    public static float[] getScopesAverageVoltage(Waveform waveform, float readVoltage, int pulseWidthInMicroSeconds, int dWFWaveformChannel)
    {
        _samplesPerPulse = 300;
        _sampleFrequency = (int)(1.0 / (pulseWidthInMicroSeconds * 2 * 1E-6));
        _samples = _sampleFrequency * _samplesPerPulse;
        //Debug.Log(waveform.ToString() + " freq: " + _sampleFrequency);
        MemristorController.StartAnalogCaptureBothChannelsTriggerOnWaveformGenerator(dWFWaveformChannel, _samples, _samplesPerPulse, true);
        MemristorController.WaitUntilArmed();
        double[] pulse = WaveformUtils.GenerateCustomWaveform(waveform, readVoltage, _sampleFrequency);
        MemristorController.StartCustomPulseTrain(dWFWaveformChannel, _sampleFrequency, 0, 1, pulse);

        bool success = MemristorController.CapturePulseData(_samples, 1);
        if (success)
        {
            int validSamples = Dwf.AnalogInStatusSamplesValid(MemristorController.hdwf);
            double[] v1 = Dwf.AnalogInStatusData(MemristorController.hdwf, (int)CHANNELS.CHANNEL_1, validSamples);
            double[] v2 = Dwf.AnalogInStatusData(MemristorController.hdwf, (int)CHANNELS.CHANNEL_2, validSamples);

            /*
             * Note from Alex: The output is a pulse with the last half of the measurement data at ground. Taking the first 50% insures we get the pulse
             * amplitude.
             */

            double aveScope1 = 0;
            double aveScope2 = 0;

            for (int i = 0; i < v1.Length / 2; i++)
            {
                aveScope1 += v1[i];
                aveScope2 += v2[i];
            }

            aveScope1 /= v1.Length / 2;
            aveScope2 /= v2.Length / 2;

            return new float[] { (float)aveScope1, (float)aveScope2 };

        }
        else
        {
            return null;
        }
    }

    public static void DCExperiment(int memristorId, Waveform readWaveform, Waveform writeEraseWaveform, float V_WRITE, float V_ERASE, float V_READ, int READ_PULSE_WIDTH_IN_MICRO_SECONDS, int WRITE_PULSE_WIDTH_IN_MICRO_SECONDS, int ERASE_PULSE_WIDTH_IN_MICRO_SECONDS)
    {
        //REFACTOR QUESTION: THE USAGE OF SUCCES IS INCONSISTENT ACROSS EXPERIMENTS, INCLUDING IF WE SHOULD STOPWAVE and ANALOGCAPTUREBOTHCHANNELS AFTER EACH PULSE/EXPERIMENT

        //refactor graph access via singleton
        var graph = UIManager.Panels[Experiments.DC].GetComponentInChildren<LineGraphContinuous2D>();

        //GET VALUES FROM UI AND UPDATE MODEL
        //https://github.com/knowm/memristor-discovery/blob/e414f89b15aeba3ef2a6d21965b071162f6f3189/src/main/java/org/knowm/memristor/discovery/gui/mvc/experiments/dc/DCPreferences.java
        Model.SetWaveform(Waveform.TriangleUpDown);
        Model.SetPulseNumber(1);
        Model.SetAmplitude(0.8f);
        Model.SetPeriod(500); //500 milliseconds, see timeunits
        Model.CURRENT_UNIT = CurrentUnits.MicroAmps;
        Model.RESISTANCE_UNIT = ResistanceUnits.KiloOhms;
        Model.CONDUCTANCE_UNIT = ConductanceUnits.MilliSiemens;
        Model.TIME_UNIT = TimeUnits.MilliSeconds;

        // ////////////////////////////////
        // Analog In /////////////////
        // ////////////////////////////////

        int samplesPerPulse = 200; // adjust this down if you want to capture more pulses as the buffer size is limited.
        double sampleFrequency = Model.GetCalculatedFrequencyDC() * samplesPerPulse;
        double samples = sampleFrequency * samplesPerPulse;

        MemristorController.StartAnalogCaptureBothChannelsLevelTrigger(sampleFrequency, -0.02 * (Model.GetAmplitude() > 0 ? 1 : -1), samplesPerPulse * Model.GetPulseNumber());
        MemristorController.WaitUntilArmed();

        // ////////////////////////////////
        // Pulse Out /////////////////
        // ////////////////////////////////

        double[] customWaveform;

        customWaveform = WaveformUtils.GenerateCustomWaveform(Model.GetWaveform(), -Model.GetAmplitude(), Model.GetCalculatedFrequencyDC());

        MemristorController.StartCustomPulseTrain((int)CHANNELS.CHANNEL_1, Model.GetCalculatedFrequencyDC(), 0, Model.GetPulseNumber(), customWaveform);

        // ////////////////////////////////
        // ////////////////////////////////

        // Read In Data
        bool success = MemristorController.CapturePulseData(Model.GetCalculatedFrequencyDC(), Model.GetPulseNumber());

        if (success)
        {
            // Get Raw Data from Oscilloscope
            int validSamples = Dwf.AnalogInStatusSamplesValid(MemristorController.hdwf);

            double[] v1 = Dwf.AnalogInStatusData(MemristorController.hdwf, (int)CHANNELS.CHANNEL_1, validSamples);
            double[] v2 = Dwf.AnalogInStatusData(MemristorController.hdwf, (int)CHANNELS.CHANNEL_2, validSamples);

            // /////////////////////////
            // Create Chart Data //////
            // /////////////////////////

            double[] VMemristor;

            //invert v1 and v2 data
            var v2i = PostProcessHelper.Invert(v2);
            VMemristor = PostProcessHelper.Invert(v1);
            int bufferLength = v1.Length;

            // create time data
            double[] timeData = new double[bufferLength];
            double timeStep = 1 / sampleFrequency * TimeUnitsHelper.GetDivisor(Model.TIME_UNIT);
            for (int i = 0; i < bufferLength; i++)
            {
                timeData[i] = i * timeStep;
            }

            // create current data
            double[] current = new double[bufferLength];
            double[] dv = new double[bufferLength];
            double[] vRMAccum = new double[bufferLength]; //voltage Resistor+Memristor Accumulated
            double[] dcurrentAccum = new double[bufferLength];
            vRMAccum[0] = 0;
            dcurrentAccum[0] = 0;

            for (int i = 0; i < bufferLength; i++)
            {
                dv[i] = (v1[i] - v2[i]);
                vRMAccum[i] += v2[i];

                current[i] = dv[i] / MemristorController.SERIES_RESISTANCE * CurrentUnitsHelper.GetDivisor(Model.CURRENT_UNIT);
                dcurrentAccum[i] += current[i];
            }

            // create conductance data
            double[] conductance = new double[bufferLength];
            for (int i = 0; i < bufferLength; i++)
            {
                double I = dv[i] / MemristorController.SERIES_RESISTANCE;
                double G = I / (-v1[i]) * ConductanceUnitsHelper.GetDivisor(Model.CONDUCTANCE_UNIT);
                G = G < 0 ? 0 : G;
                conductance[i] = G;
            }

            for (int i = 0; i < timeData.Length; i++)
            {
                graph.AddDataPointToLine(memristorId, new Vector2((float)vRMAccum[i], (float)dcurrentAccum[i]));

                Logger.dataQueue.Add(string.Format("MID;{0};T{1};vMR{2};VM;{3};Current;{4};Conductance;{5};deltaV;{6};vAccum;{7};dcurrentAccum;{8}",
                                                    memristorId, timeData[i], v2i[i], VMemristor[i], current[i], conductance[i], dv[i], vRMAccum[i], dcurrentAccum[i]));
            }

            //add final logging data
            Logger.dataQueue.Add(string.Format("Read Waveform;{0};WriteErase Waveform;{1};Samples per pulse;{2};SampleFrequency;{3};Samples{4};Amplitude;{5};Pulse numbers;{6};Period{7};Period timeunit;{8}", "---", writeEraseWaveform.ToString(), samplesPerPulse, sampleFrequency, samples, Model.GetAmplitude(), Model.GetPulseNumber(), Model.GetPeriod(), TimeUnitsHelper.GetLabel(Model.TIME_UNIT)));
        }
        else
        {
            // Stop Analog In and Out
            MemristorController.StopWave((int)CHANNELS.CHANNEL_1);
            MemristorController.StopAnalogCaptureBothChannels();
        }
    }

   

    internal static void ReadAll(uint digitalIOStates)
    {
        //write results to logger! Incl. expected value.

        Scheduler.IsProcessIdle = true;
    }

    public static void ReadSingle2(int id)
    {
        Model.SetAmplitude(MemristorController.V_READ);
        var resistance = getSwitchResistancekOhm(Waveform.Square, (float) -Model.GetAmplitude(), MemristorController.PULSE_WIDTH_IN_MICRO_SECONDS, (int)CHANNELS.CHANNEL_1);

        int state = GetGroundTruth(id);
        var actualTrit = ConvertOhmToTrit((float)resistance);
        Logger.dataQueue.Add(string.Format("STATUS;{0};ACTION;{1};ACTUAL;{2};GROUNDTRUTH;{3};K_OHMS;{4};LowerTHRESHOLD;{5};UpperTHRESHOLD;{6};D_ToLowerThreshold;{7};D_ToUpperhreshold;{8};", (actualTrit == state ? "OK" : "FAIL"), PulseType.Read.ToString(), actualTrit, state, resistance, OutputController.LowerLimitState, OutputController.UpperLimitState, Math.Abs(resistance - OutputController.LowerLimitState), Math.Abs(resistance - OutputController.UpperLimitState)));

        //Update value (don't wait for read interval)
        MemristorController.Output.Enqueue(string.Format("{0},{1}", id, actualTrit));

        Scheduler.IsProcessIdle = true;

    }

    public static void ReadSingle(int id)
    {
        //the returned value
        double averageCurrentInOhms = 0;

        //GET VALUES FROM UI AND UPDATE MODEL
        //https://github.com/knowm/memristor-discovery/blob/e414f89b15aeba3ef2a6d21965b071162f6f3189/src/main/java/org/knowm/memristor/discovery/gui/mvc/experiments/dc/DCPreferences.java
        Model.SetWaveform(Waveform.Square);
        Model.SetPulseNumber(1);
        Model.SetAmplitude(0.1f);
        Model.SetPeriod(500); //500 milliseconds, see timeunits
        Model.CURRENT_UNIT = CurrentUnits.MicroAmps;
        Model.RESISTANCE_UNIT = ResistanceUnits.KiloOhms;
        Model.CONDUCTANCE_UNIT = ConductanceUnits.MilliSiemens;
        Model.TIME_UNIT = TimeUnits.MilliSeconds;
       
      
    // ////////////////////////////////
    // Analog In /////////////////
    // ////////////////////////////////

    // trigger on 20% the rising .1 V read pulse
    int samplesPerPulse = 300;
        double f = 1 / Model.GetReadPulseWidth();
        double sampleFrequency = f * samplesPerPulse;

        MemristorController.StartAnalogCaptureBothChannelsTriggerOnWaveformGenerator((int)CHANNELS.CHANNEL_1, sampleFrequency, samplesPerPulse, true);
        MemristorController.WaitUntilArmed();

        //////////////////////////////////
        // Pulse Out /////////////////
        //////////////////////////////////

        // bug in original? the read pulse width was 25*2 =50 us, using that now. Refactor to simply use pulse width?
        // read pulse: 0.1 V, 5(50!) us pulse width
        double[] customWaveform = WaveformUtils.GenerateCustomWaveform(Model.GetWaveform(), -Model.GetAmplitude(), f);
        MemristorController.StartCustomPulseTrain((int)CHANNELS.CHANNEL_1, f, 0, Model.GetPulseNumber(), customWaveform);
        
        // Read In Data
        bool success = MemristorController.CapturePulseData(f, Model.GetPulseNumber());

        if (success)
        {
            // Get Raw Data from Oscilloscope
            int validSamples = Dwf.AnalogInStatusSamplesValid(MemristorController.hdwf);

            double[] v1 = Dwf.AnalogInStatusData(MemristorController.hdwf, (int)CHANNELS.CHANNEL_1, validSamples);
            double[] v2 = Dwf.AnalogInStatusData(MemristorController.hdwf, (int)CHANNELS.CHANNEL_2, validSamples);
            
            // /////////////////////////
            // Create Chart Data //////
            // /////////////////////////

            var trimmedRawData = PostProcessHelper.TrimIdleData(v1, v2, 0, 10);
            var V1Trimmed = trimmedRawData[0];
            var V2Trimmed = trimmedRawData[1];

            var VMemristor = PostProcessHelper.Invert(V1Trimmed);
           
            var bufferLength = V1Trimmed.Length;

            // create time data
            var timeData = new double[bufferLength];
            var timeStep = 1.0 / sampleFrequency * TimeUnitsHelper.GetDivisor(Model.TIME_UNIT);

            for (int i = 0; i < bufferLength; i++)
            {
                timeData[i] = i * timeStep;
            }

            //get the voltage of V2 right before pulse falling/rising edge. This is given to the RC Computer to get the resistance.
            double resistance=0;

            if (Model.UseSpiceSimulator)
            {
                //NOT IMPLEMENTED YET, SEE JSPICE REPO IN DIRECTORY

                //double vRead = V1Trimmed[V1Trimmed.Length / 3]; // best guess
                //for (int i = 50; i < V1Trimmed.Length; i++)
                //{
                //    double pD = (V2Trimmed[i] - V2Trimmed[i - 1]) / V2Trimmed[i];

                //    if (pD > .05)
                //    {
                //        vRead = V1Trimmed[i - 5];
                //        break;
                //    }
                //}

                //resistance = Model.GetRcComputer().GetRFromV(vRead);
            }
            else
            {
                // create current data
                double[] current = new double[bufferLength];
                double[] dcurrentAccum = new double[bufferLength];
                dcurrentAccum[0] = 0;

                for (int i = 0; i < bufferLength; i++)
                {
                    current[i] = (V1Trimmed[i] - V2Trimmed[i]) / MemristorController.SERIES_RESISTANCE * CurrentUnitsHelper.GetDivisor(Model.CURRENT_UNIT);
                    dcurrentAccum[i] += current[i];
                }

                //we need to summerize all data points into 1 data point, take last
                resistance = dcurrentAccum[dcurrentAccum.Length - 1];
            }

            //calculate conductance (G = 1/R)
            //double[] conductanceAve = new double[] { (1 / resistance) * ConductanceUnitsHelper.GetDivisor(Model.CONDUCTANCE_UNIT)};

            int state = GetGroundTruth(id);
            var actualTrit = ConvertOhmToTrit((float) resistance);
            Logger.dataQueue.Add(string.Format("STATUS;{0};ACTION;{1};ACTUAL;{2};GROUNDTRUTH;{3};K_OHMS;{4};LowerTHRESHOLD;{5};UpperTHRESHOLD;{6};D_ToLowerThreshold;{7};D_ToUpperhreshold;{8};", (actualTrit == state ? "OK" : "FAIL"), PulseType.Read.ToString(), actualTrit, state, resistance, OutputController.LowerLimitState, OutputController.UpperLimitState, Math.Abs(resistance - OutputController.LowerLimitState), Math.Abs(resistance - OutputController.UpperLimitState)));

            //Update value (don't wait for read interval)
            MemristorController.Output.Enqueue(string.Format("{0},{1}", id, actualTrit));
        }
        else
        {
            // Stop Analog In and Out
            MemristorController.StopWave((int)CHANNELS.CHANNEL_1);
            MemristorController.StopAnalogCaptureBothChannels();
            Debug.LogError("WritePulse Failed, do not rely on readings");
        }
       
        // Stop Analog In and Out
        MemristorController.StopWave((int)CHANNELS.CHANNEL_1);
        MemristorController.StopAnalogCaptureBothChannels();
    }
    
    private static int GetGroundTruth(int id)
    {
        var state = InputController.GroundTruth[id-1];
        return state;
    }

    public static int ConvertOhmToTrit(float read)
    {
        if (read > OutputController.UpperLimitState)
            return 0;

        if (read < OutputController.LowerLimitState)
            return 2;
        
        return 1;
    }
    
    public static float SingleWritePulse(int id, PulseType type)
    {
        //the returned value
        double averageCurrentInOhms = 0;

        //GET VALUES FROM UI AND UPDATE MODEL
        //https://github.com/knowm/memristor-discovery/blob/e414f89b15aeba3ef2a6d21965b071162f6f3189/src/main/java/org/knowm/memristor/discovery/gui/mvc/experiments/dc/DCPreferences.java
        if (type == PulseType.WriteState1)
        {
            Model.SetAmplitude(0.3f);
        }

        if (type == PulseType.WriteState2)
        {
            Model.SetAmplitude(1f);
        }

        if (type == PulseType.Erase)
        {
            Model.SetAmplitude(-1f);
        }


        Model.SetWaveform(Waveform.SquareSmooth);
        Model.SetPulseNumber(1);
        Model.SetAmplitude(1f);
        Model.SetPeriod(500); //500 milliseconds, see timeunits
        Model.CURRENT_UNIT = CurrentUnits.MicroAmps;
        Model.RESISTANCE_UNIT = ResistanceUnits.KiloOhms;
        Model.CONDUCTANCE_UNIT = ConductanceUnits.MilliSiemens;
        Model.TIME_UNIT = TimeUnits.MilliSeconds;

        // ////////////////////////////////
        // Analog In /////////////////
        // ////////////////////////////////

        int samplesPerPulse = 200;
        double sampleFrequency = Model.GetCalculatedFrequencyPulse() * samplesPerPulse;
        bool isScale2V = Math.Abs(Model.GetAmplitude()) <= 2.5;
        int bufferSize = samplesPerPulse * Model.GetPulseNumber() + samplesPerPulse;

        MemristorController.StartAnalogCaptureBothChannelsTriggerOnWaveformGenerator((int)CHANNELS.CHANNEL_1, sampleFrequency, bufferSize, isScale2V);
        MemristorController.WaitUntilArmed();

        // ////////////////////////////////
        // Pulse Out /////////////////
        // ////////////////////////////////

        double[] customWaveform = WaveformUtils.GenerateCustomPulse(Model.GetWaveform(), -Model.GetAmplitude(), Model.GetPulseWidth(), Model.GetDutyCycle());
        MemristorController.StartCustomPulseTrain((int)CHANNELS.CHANNEL_1, Model.GetCalculatedFrequencyPulse(), 0, Model.GetPulseNumber(), customWaveform);

        // ////////////////////////////////

        // Read In Data
        bool success = MemristorController.CapturePulseData(Model.GetCalculatedFrequencyPulse(), Model.GetPulseNumber());

        if (success)
        {
            // Get Raw Data from Oscilloscope
            int validSamples = Dwf.AnalogInStatusSamplesValid(MemristorController.hdwf);

            double[] v1 = Dwf.AnalogInStatusData(MemristorController.hdwf, (int)CHANNELS.CHANNEL_1, validSamples);
            double[] v2 = Dwf.AnalogInStatusData(MemristorController.hdwf, (int)CHANNELS.CHANNEL_2, validSamples);

            // Stop Analog In and Out
            MemristorController.StopWave((int)CHANNELS.CHANNEL_1);
            MemristorController.StopAnalogCaptureBothChannels();

            // /////////////////////////
            // Create Chart Data //////
            // /////////////////////////

            double[][] trimmedRawData = PostProcessHelper.TrimIdleData(v1, v2, 0.05, 10);
            double[] V1Trimmed = trimmedRawData[0];
            double[] V2Trimmed = trimmedRawData[1];
            double[] VMemristor = PostProcessHelper.Invert(V1Trimmed);
            double[] timeData;
            int bufferLength;
            double timeStep;

            bufferLength = V1Trimmed.Length;
            
            // create time data
            timeData = new double[bufferLength];
            timeStep = 1.0 / sampleFrequency * TimeUnitsHelper.GetDivisor(Model.TIME_UNIT);
            for (int i = 0; i < bufferLength; i++)
            {
                timeData[i] = i * timeStep;
            }

            // create current data
            double[] current = new double[bufferLength];
            double[] dcurrentAccum = new double[bufferLength];
            dcurrentAccum[0] = 0;

            for (int i = 0; i < bufferLength; i++)
            {
                current[i] = (V1Trimmed[i] - V2Trimmed[i]) / MemristorController.SERIES_RESISTANCE * CurrentUnitsHelper.GetDivisor(Model.CURRENT_UNIT);
                dcurrentAccum[i] += current[i];
            }

            // create conductance data
            double[] conductance = new double[bufferLength];
            for (int i = 0; i < bufferLength; i++)
            {
                double I = (V1Trimmed[i] - V2Trimmed[i]) / MemristorController.SERIES_RESISTANCE;
                double G = I / VMemristor[i] * ConductanceUnitsHelper.GetDivisor(Model.CONDUCTANCE_UNIT);
                G = G < 0 ? 0 : G;
                conductance[i] = G;
            }

            //we need to summerize all data points into 1 data point, take last
            averageCurrentInOhms = dcurrentAccum[dcurrentAccum.Length - 1];
        }
        else
        {
            // Stop Analog In and Out
            MemristorController.StopWave((int)CHANNELS.CHANNEL_1);
            MemristorController.StopAnalogCaptureBothChannels();
            Debug.LogError("WritePulse Failed, do not rely on readings");
        }

        return (float)averageCurrentInOhms;
    }

    public static void SingleWrite2(int id, PulseType type, float voltage)
    {
        switch (type)
        {
            case PulseType.WriteState1:
                Model.SetAmplitude(voltage);
                Model.SetWaveform(Waveform.HalfSine);
                break;
            case PulseType.WriteState2:
                Model.SetAmplitude(voltage);
                Model.SetWaveform(Waveform.HalfSine);
                break;
            case PulseType.Erase:
                Model.SetAmplitude(voltage);
                Model.SetWaveform(Waveform.HalfSine);
                break;
        }

        getSwitchResistancekOhm(Model.GetWaveform(), (float)-Model.GetAmplitude(), MemristorController.PULSE_WIDTH_IN_MICRO_SECONDS, (int)CHANNELS.CHANNEL_1);
    }

    public static void WriteSingle(int id, int state)
    {
        var stateReached = false; //read value matches desired state 
        var tries = 0; //max 3 tries;
        float voltage = 0.3f;//default is -0.4f, since in the actual call negates this voltage. Refactor, confusing!
        while (stateReached == false)
        {
            PulseType action;

            switch (state)
            {
                case 0:
                {
                    action = PulseType.Erase;
                    //resistance = SingleWritePulse(id, action);
                    SingleWrite2(id, action, MemristorController.V_RESET);
                }
                    break;
                case 1:
                {
                    action = PulseType.WriteState1;
                    //resistance = SingleWritePulse(id, action);
                    SingleWrite2(id, action, voltage);
                    }
                    break;
                case 2:
                {
                    action = PulseType.WriteState2;
                    //resistance = SingleWritePulse(id, action);
                    SingleWrite2(id, action, MemristorController.V_WRITE);
                }
                    break;
                default:
                    action = PulseType.Error;
                    Debug.Log("Error in WriteSingle");
                    break;
            }

            Thread.Sleep(25);

            //do read pulse
            var resistance = getSwitchResistancekOhm(Waveform.Square, -MemristorController.V_READ, MemristorController.PULSE_WIDTH_IN_MICRO_SECONDS, (int)CHANNELS.CHANNEL_1);

            Thread.Sleep(25);

            var actualTrit = ConvertOhmToTrit(resistance);
            Logger.dataQueue.Add(string.Format("STATUS;{0};ACTION;{1};ACTUAL;{2};GROUNDTRUTH;{3};K_OHMS;{4};LowerTHRESHOLD;{5};UpperTHRESHOLD;{6};D_ToLowerThreshold;{7};D_ToUpperhreshold;{8};TRY;{9}", (actualTrit == state ? "OK" : "FAIL"), action.ToString(), actualTrit, state, resistance, OutputController.LowerLimitState, OutputController.UpperLimitState, Math.Abs(resistance - OutputController.LowerLimitState), Math.Abs(resistance - OutputController.UpperLimitState), tries));

            
            if (actualTrit == state) //maybe build in a feature to check if resistance is close to optimal and have a buffer? Now if it is at tipping point, if succeeds but will flicker
            {
                stateReached = true;
                MemristorController.Output.Enqueue(string.Format("{0},{1}", id, actualTrit));
            }
            else
            {
                tries++;

                //change polarity if direction of pulse is wrong, only applies to state 1, being middle
                if (state == 1)
                {
                    if (resistance <= OutputController.LowerLimitState)
                        voltage = -0.3f;
                    else
                        voltage = 0.3f;
                }

                if (tries >= 3) //we fail and stop trying
                {
                    stateReached = true;
                    MemristorController.Output.Enqueue(string.Format("{0},{1}", id, actualTrit));
                }
            }
        }

        Scheduler.IsProcessIdle = true;

    }

    public static void EraseMemristorStates(Waveform readWaveform, Waveform writeEraseWaveform, float V_WRITE, float V_ERASE, float V_READ, int READ_PULSE_WIDTH_IN_MICRO_SECONDS, int WRITE_PULSE_WIDTH_IN_MICRO_SECONDS, int ERASE_PULSE_WIDTH_IN_MICRO_SECONDS)
    {
        measureAllSwitchResistances(writeEraseWaveform, V_ERASE, ERASE_PULSE_WIDTH_IN_MICRO_SECONDS);
        Thread.Sleep(25);
    }

    public static void EraseSingleMemristor(int id, Waveform eraseWave, float eraseVoltage, int erasePulseWidth)
    {
        MemristorController.ToggleMemristor(id, true);

        //do erase twice
        for (int i = 0; i < 2; i++)
        {
            //do erase (read resistance from erase/write is invalid, so use read pulses only)
            getSwitchResistancekOhm(eraseWave, eraseVoltage, erasePulseWidth, (int)CHANNELS.CHANNEL_1);
            Thread.Sleep(25);

            //do read DONT FORGET TO PUT -sign for each voltage level, we should refactor this.
            var resistance = getSwitchResistancekOhm(Waveform.Square, -MemristorController.V_READ, MemristorController.PULSE_WIDTH_IN_MICRO_SECONDS, (int)CHANNELS.CHANNEL_1);
            Logger.dataQueue.Add("Erase " + i + " resistance= " + resistance);
        }

        MemristorController.ToggleMemristor(id, false);

    }
}
