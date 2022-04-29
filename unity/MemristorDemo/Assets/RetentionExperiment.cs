using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using UnityEngine;
using static AD2Scheduler;
using static ExperimentManager;
using static MemristorController;
using System.Diagnostics;
using Tayx.Graphy.Utils.NumString;
using System.Threading;
using WaveFormsSDK;
public class RetentionExperiment : MonoBehaviour
{
    public enum Intervals
    {
        Freq10Hz =0, //0.1 sec
        Freq1Hz = 1, //1 sec
        Freq1_60Hz = 2, //60 sec
        Freq1_600Hz = 3 //600 sec
    }

    public Intervals ReadingIntervalInSec = Intervals.Freq1Hz;
    public static Intervals Interval;
    public static int GraphLineId;
    private float frequency;
    float timePassed = 0;
    public GameObject horizontalBar1Hz; //refactor this in graph settings
    public GameObject horizontalBar1_60Hz; //refactor this in graph settings

    [Range (1,16)]
    public int memristorId = 1;
    int n = 0;
    List<Threshold> states = new List<Threshold>();
    public int N = 10; //read datapoints
    int currentThresholdIndex = 0;
    public Threshold[] thresholds = new Threshold[9];

    [System.Serializable]
    public class Threshold
    {
        //WRITE
        [Range(-2.0f, 1f)]
        public float pulseAmplitude = 1;
        [Range(1, 50000)] //im not sure if the analog discovery 2 or pro can send 10 ns second pulse width. It can do 100ns (10 Mhz), maybe refactor to nano seconds  
        public int pulseWidthWriteInUs = 50000; //50_000 is default = freq of 10 Hz in waveform or 1 pulse of 50 ms on and 50 ms off
        [Range(1, 50)]
        public int pulseAmountWrite = 1;
        public Waveform pulseShapeWrite = Waveform.HalfSine;

        //READ
        [Range(-0.2f, 0.2f)]
        public float readAmplitude = .1f;
        [Range(1, 50000)] //im not sure if the analog discovery 2 or pro can send 10 ns second pulse width. It can do 100ns (10 Mhz), maybe refactor to nano seconds  
        public int pulseWidthReadInUs = 50000; //50_000 is default = freq of 10 Hz in waveform or 1 pulse of 50 ms on and 50 ms off
        [Range(1, 50)]
        public int pulseAmountRead = 1;
        public Waveform pulseShapeRead  = Waveform.Square;

        //DELAY AFTER EACH READ OR WRITE ACTION
        [Range(1, 50)]
        public int delayAfterReadOrWriteInMs = 25;
    }

    public void StartExperiment()
    {
        Interval = ReadingIntervalInSec;
        var date = DateTime.Today.ToShortDateString();
        var time = DateTime.Now.ToShortTimeString();
     
        MemristorController.Stopwatch.Start();

        switch (Interval)
        {
            case Intervals.Freq1Hz:
                horizontalBar1Hz.SetActive(true);
                horizontalBar1_60Hz.SetActive(false);
                frequency = 1;
                break;
            case Intervals.Freq1_60Hz:
                horizontalBar1Hz.SetActive(false);
                horizontalBar1_60Hz.SetActive(true);
                frequency = 60;
                break;
        }

        //HEADER
        var experiment = "Retention Experiment";
        var settings = string.Format("DATE: {0}  TIME: {5} V_WRITE: {1}v  V_RESET: {2}v  V_READ {3}v  SERIES_RES {4}Ω", date, V_WRITE, V_RESET, V_READ, SERIES_RESISTANCE, time);
        Logger.dataQueue.Add(experiment);
        Logger.dataQueue.Add(settings);

        //determine states to be processed
        for (int i = 0; i < thresholds.Length; i++)
        {
            states.Add(thresholds[i]);
        }

        //We trigger the mux to select the correct memristor
        ToggleMemristor(memristorId - 1, true);

        //Start with erase
        //for (int i = 0; i < 2; i++)
        //{
        //    //do erase (read resistance from erase/write is invalid, so use read pulses only)
        //    PulseUtility.getSwitchResistancekOhm(Waveform.HalfSine, -V_RESET, PULSE_WIDTH_IN_MICRO_SECONDS, (int)CHANNELS.CHANNEL_1);
        //    Thread.Sleep(25);

        //    //do read DONT FORGET TO PUT -sign for each voltage level, we should refactor this.
        //    var resistance = PulseUtility.getSwitchResistancekOhm(Waveform.Square, -MemristorController.V_READ, MemristorController.PULSE_WIDTH_IN_MICRO_SECONDS, (int)CHANNELS.CHANNEL_1);
        //    Thread.Sleep(25);
        //    Logger.dataQueue.Add("Erase " + i + " resistance= " + resistance);
        //}
        //MemristorController.Stopwatch.Restart();

        StartScheduler();
    }

  
    // Update is called once per frame
    public void Update()
    {
        if (ExperimentManager.experiment == ExperimentManager.Experiments.Retention)
        {
            if (ExperimentManager.status == ExperimentStatus.Started)
            {
               //Write once
               if(n==0)
               {
                    //for (int i = 0; i < 1; i++)
                    //{
                    //    //do erase (read resistance from erase/write is invalid, so use read pulses only)
                    //    PulseUtility.getSwitchResistancekOhm(Waveform.HalfSine, -V_RESET, PULSE_WIDTH_IN_MICRO_SECONDS, (int)CHANNELS.CHANNEL_1);
                    //    Thread.Sleep(25);

                    //    //do read DONT FORGET TO PUT -sign for each voltage level, we should refactor this.
                    //    var resistance = PulseUtility.getSwitchResistancekOhm(Waveform.Square, -MemristorController.V_READ, MemristorController.PULSE_WIDTH_IN_MICRO_SECONDS, (int)CHANNELS.CHANNEL_1);
                    //    Thread.Sleep(25);
                    //    Logger.dataQueue.Add("Erase " + i + " resistance= " + resistance);
                    //}
                        Model.SetAmplitude(states[currentThresholdIndex].pulseAmplitude);
                        Model.SetWaveform(states[currentThresholdIndex].pulseShapeWrite);
                        Model.SetPulseNumber(states[currentThresholdIndex].pulseAmountWrite);
                        PulseUtility.getSwitchResistancekOhm(Model.GetWaveform(), (float)-Model.GetAmplitude(), states[currentThresholdIndex].pulseWidthWriteInUs, (int)CHANNELS.CHANNEL_1);
                        Logger.dataQueue.Add("ACTION;WRITE;STATE;" + currentThresholdIndex);
                        MemristorController.Stopwatch.Restart();
                        timePassed = frequency;
                        GraphLineId = currentThresholdIndex;
                        Thread.Sleep(25);
               }

                //Start reading pulse every x second
                if ((timePassed >= frequency) && (n < N))
                {
                    n++;
                    timePassed = 0;
                    MemristorController.Scheduler.Schedule(new AD2Instruction(AD2Instructions.ReadSingleWithoutMux, memristorId, currentThresholdIndex, states[currentThresholdIndex].readAmplitude, states[currentThresholdIndex].pulseWidthReadInUs, states[currentThresholdIndex].pulseShapeRead, states[currentThresholdIndex].pulseAmountRead));
                }

                timePassed += Time.deltaTime;

                //Stop condition
                if (n == N)
                {
                    if (currentThresholdIndex+1 < thresholds.Length)
                    {
                        currentThresholdIndex++;
                        n = 0;
                        Thread.Sleep(25);
                    }
                    else //Experiment done
                    {
                        ExperimentManager.status = ExperimentStatus.NotReady;
                    }
                }
                
            }
        }
    }
}
