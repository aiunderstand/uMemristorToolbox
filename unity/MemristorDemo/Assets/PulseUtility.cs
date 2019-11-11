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

public class PulseUtility
{
    /*
     * measures starting resistance values writes devices, measure resistance values erase, measure resistance values returns array of resistance values
     * for each device (in kOhms): start, write, erase
     */
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

            Logger.dataQueue.Add(string.Format("Read Waveform {0}  WriteErase Waveform {1}  samples per pulse {2}  sampleFrequency {3}  samples{4}", Waveform.Square.ToString(), Waveform.HalfSine.ToString(),_samplesPerPulse,_sampleFrequency,_samples));

            return reads;


        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void ReadExperiment(Waveform readWaveform, Waveform writeEraseWaveform, float V_WRITE, float V_ERASE, float V_READ, int READ_PULSE_WIDTH_IN_MICRO_SECONDS, int WRITE_PULSE_WIDTH_IN_MICRO_SECONDS, int ERASE_PULSE_WIDTH_IN_MICRO_SECONDS)
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
            Logger.dataQueue.Add(string.Format("Read Waveform {0}  WriteErase Waveform {1}  samples per pulse {2}  sampleFrequency {3}  samples{4}", readWaveform.ToString(), writeEraseWaveform.ToString(), _samplesPerPulse, _sampleFrequency, _samples));            
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
            Logger.dataQueue.Add(string.Format("Read Waveform {0}  WriteErase Waveform {1}  samples per pulse {2}  sampleFrequency {3}  samples{4}", readWaveform.ToString(), writeEraseWaveform.ToString(), _samplesPerPulse, _sampleFrequency, _samples));
        }
        catch (Exception)
        {
        }
    }
    
    public static float[] measureAllSwitchResistances(
        Waveform waveform, float readVoltage, int pulseWidthInMicroSeconds)
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
            Debug.Log("WARNING: Voltage drop across series resistor (" + vMeasure[1]+ ") is at or below noise threshold.");
            return float.PositiveInfinity;
        }

        float I = (vMeasure[1] - vMeasure[0]) / MemristorController.SERIES_RESISTANCE;
        float rSwitch = Math.Abs(vMeasure[0] / I);

        return rSwitch / 1000; // to kilohms        
    }

    public static float[] getScopesAverageVoltage(
        Waveform waveform, float readVoltage, int pulseWidthInMicroSeconds, int dWFWaveformChannel)
    {
        _samplesPerPulse = 300;
        _sampleFrequency = (int)(1.0 / (pulseWidthInMicroSeconds * 2 * 1E-6));
        _samples = _sampleFrequency * _samplesPerPulse;

        MemristorController.StartAnalogCaptureBothChannelsTriggerOnWaveformGenerator(dWFWaveformChannel, _samples, _samplesPerPulse, true);
        MemristorController.WaitUntilArmed();
        double[] pulse = WaveformUtils.generateCustomWaveform(waveform, readVoltage, _sampleFrequency);
        MemristorController.StartCustomPulseTrain(dWFWaveformChannel, _sampleFrequency, 0, 1, pulse);

        bool success = MemristorController.CapturePulseData(_samples, 1);
        if (success)
        {
            int validSamples = Dwf.AnalogInStatusSamplesValid(MemristorController.hdwf);
            double[] v1 = Dwf.AnalogInStatusData(MemristorController.hdwf, (int)CHANNELS.CHANNEL_1, validSamples);
            double[] v2 = Dwf.AnalogInStatusData(MemristorController.hdwf, (int) CHANNELS.CHANNEL_2, validSamples);

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

        MemristorController.StartAnalogCaptureBothChannelsLevelTrigger(sampleFrequency, -0.02 * (Model.GetAmplitude() > 0 ? 1 : -1),  samplesPerPulse * Model.GetPulseNumber());
        MemristorController.WaitUntilArmed();

        // ////////////////////////////////
        // Pulse Out /////////////////
        // ////////////////////////////////

        double[] customWaveform;

        customWaveform = WaveformUtils.generateCustomWaveform(Model.GetWaveform(),-Model.GetAmplitude(),Model.GetCalculatedFrequencyDC());

        MemristorController.StartCustomPulseTrain((int) CHANNELS.CHANNEL_1, Model.GetCalculatedFrequencyDC(), 0, Model.GetPulseNumber(), customWaveform);

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
                vRMAccum[i] += v1[i];

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

            //invert some data
            var v2i = PostProcessHelper.Invert(v2);
            
            for (int i = 0; i < timeData.Length; i++)
            {
                graph.AddDataPointToLine(memristorId, new Vector2((float)vRMAccum[i], (float) dcurrentAccum[i]));

                Logger.dataQueue.Add(string.Format("Memristor {0}  TimeData: {1}  V2: {2}  VMemristor: {3}  Current: {4}  Conductance: {5}  deltaV: {6}  dvAccum: {7}  dcurrentAccum: {8}",
                                                    memristorId, timeData[i], v2i[i], VMemristor[i], current [i], conductance[i], dv[i], vRMAccum[i], dcurrentAccum[i]));
            }

            //add final logging data
            Logger.dataQueue.Add(string.Format("Read Waveform {0}  WriteErase Waveform {1}  samples per pulse {2}  sampleFrequency {3}  samples{4}  amplitude {5}  pulse numbers {6}  period{7} {8}", "---", writeEraseWaveform.ToString(), samplesPerPulse, sampleFrequency, samples, Model.GetAmplitude(), Model.GetPulseNumber(), Model.GetPeriod(), TimeUnitsHelper.GetLabel(Model.TIME_UNIT)));
        }
        else
        {
            // Stop Analog In and Out
            MemristorController.StopWave( (int) CHANNELS.CHANNEL_1);
            MemristorController.StopAnalogCaptureBothChannels();
        }
    }
}