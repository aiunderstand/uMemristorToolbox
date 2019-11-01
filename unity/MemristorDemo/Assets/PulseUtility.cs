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

public class PulseUtility
{
    /*
     * measures starting resistance values writes devices, measure resistance values erase, measure resistance values returns array of resistance values
     * for each device (in kOhms): start, write, erase
     */
    static int samplesPerPulse;
    static int sampleFrequency;
    static int samples;
    static int readSamples = 10;
    static int readDelay = 1000; //miliseconds
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

            Logger.dataQueue.Add(string.Format("Read Waveform {0}  WriteErase Waveform {1}  samples per pulse {2}  sampleFrequency {3}  samples{4}", Waveform.Square.ToString(), Waveform.HalfSine.ToString(),samplesPerPulse,sampleFrequency,samples));

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
            for (int i = 0; i < readSamples; i++)
            {
                reads[0] = measureAllSwitchResistances(readWaveform, V_READ, READ_PULSE_WIDTH_IN_MICRO_SECONDS);
                Logger.dataQueue.Add(FormatResistanceArray("READ      ", reads[0]));
                Thread.Sleep(readDelay);

            }
            reads[0] = measureAllSwitchResistances(readWaveform, V_READ, READ_PULSE_WIDTH_IN_MICRO_SECONDS);
            Logger.dataQueue.Add(string.Format("Read Waveform {0}  WriteErase Waveform {1}  samples per pulse {2}  sampleFrequency {3}  samples{4}", readWaveform.ToString(), writeEraseWaveform.ToString(), samplesPerPulse, sampleFrequency, samples));            
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
            for (int i = 0; i < readSamples; i++)
            {
                reads[0] = measureAllSwitchResistances(readWaveform, V_READ, READ_PULSE_WIDTH_IN_MICRO_SECONDS);
                Logger.dataQueue.Add(FormatResistanceArray("READ      ", reads[0]));
                Thread.Sleep(readDelay);

            }
            reads[0] = measureAllSwitchResistances(readWaveform, V_READ, READ_PULSE_WIDTH_IN_MICRO_SECONDS);
            Logger.dataQueue.Add(string.Format("Read Waveform {0}  WriteErase Waveform {1}  samples per pulse {2}  sampleFrequency {3}  samples{4}", readWaveform.ToString(), writeEraseWaveform.ToString(), samplesPerPulse, sampleFrequency, samples));
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
        samplesPerPulse = 300;
        sampleFrequency = (int)(1.0 / (pulseWidthInMicroSeconds * 2 * 1E-6));
        samples = sampleFrequency * samplesPerPulse;

        MemristorController.StartAnalogCaptureBothChannelsTriggerOnWaveformGenerator(dWFWaveformChannel, samples, samplesPerPulse, true);
        MemristorController.WaitUntilArmed();
        double[] pulse = WaveformUtils.generateCustomWaveform(waveform, readVoltage, sampleFrequency);
        MemristorController.StartCustomPulseTrain(dWFWaveformChannel, sampleFrequency, 0, 1, pulse);

        bool success = MemristorController.CapturePulseData(samples, 1);
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
}