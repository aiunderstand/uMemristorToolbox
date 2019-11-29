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
using UnityScript.Scripting.Pipeline;

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
    public static int GraphLineId = 1;
    private float frequency;
    float timePassed = 0;
    public GameObject horizontalBar1Hz; //refactor this in graph settings
    public GameObject horizontalBar1_60Hz; //refactor this in graph settings

    [Range (1,16)]
    public int memristorId = 1;
    int n = 0;

    public int N = 10; //datapoints

    private bool hasPhase1Completed = false;
    private bool hasPhase2Completed = false;
    private bool hasPhase3Completed = false;

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
                break;
            case Intervals.Freq1_60Hz:
                horizontalBar1Hz.SetActive(false);
                horizontalBar1_60Hz.SetActive(true);
                break;
        }
        //HEADER
        var experiment = "Retention Experiment";
        var settings = string.Format("DATE: {0}  TIME: {5} V_WRITE: {1}v  V_RESET: {2}v  V_READ {3}v  SERIES_RES {4}Ω", date, V_WRITE, V_RESET, V_READ, SERIES_RESISTANCE, time);
        Logger.dataQueue.Add(experiment);
        Logger.dataQueue.Add(settings);

        //start with state 0
        SetState(PulseUtility.PulseType.WriteState1);
        MemristorController.Stopwatch.Restart();

        StartScheduler();
    }

    //We should use the scheduler for this...
    public void SetState(PulseUtility.PulseType state)
    {
        PulseUtility.EraseSingleMemristor(memristorId-1, Waveform.HalfSine, -V_RESET, PULSE_WIDTH_IN_NANO_SECONDS);

        ToggleMemristor(memristorId-1, true);
       
        float voltage = 0;
        
        switch (state)
        {
            case PulseUtility.PulseType.Erase:
                voltage = V_RESET;

                break;
            case PulseUtility.PulseType.WriteState1:
                voltage = 0.4f;
                break;
            case PulseUtility.PulseType.WriteState2:
                voltage = V_WRITE;
                break;
        }
        
        PulseUtility.SingleWrite2(memristorId-1, state, voltage);
        Logger.dataQueue.Add("ACTION;WRITE;STATE;" + (int) state);
        ToggleMemristor(memristorId-1, false);
    }

    // Start is called before the first frame update
    public void Start()
    {
        switch (ReadingIntervalInSec)
        {
            case Intervals.Freq1Hz:
                frequency = 1;
                break;
            case Intervals.Freq10Hz:
                frequency = 0.1f;
                break;
            case Intervals.Freq1_60Hz:
                frequency = 60;
                break;
            case Intervals.Freq1_600Hz:
                frequency = 600; 
                break;
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if (ExperimentManager.status == ExperimentStatus.Started)
        {
            if (!hasPhase1Completed)
            {
                //Start reading pulse every x second
                if (timePassed > frequency)
                {
                    if (n >= N)
                    {
                        hasPhase1Completed = true;
                        InitPhase2();
                    }
                    else
                    {
                        n++;
                        timePassed = 0;
                        MemristorController.Scheduler.Schedule(new AD2Instruction(AD2Instructions.ReadSingle, memristorId,1));
                    }
                }

                timePassed += Time.deltaTime;
            }
            else
            {
                if (!hasPhase2Completed)
                {
                    //Start reading pulse every x second
                    if (timePassed > frequency)
                    {
                        if (n >= N)
                        {
                            hasPhase2Completed = true;
                            InitPhase3();
                        }
                        else
                        {
                            n++;
                            timePassed = 0;
                            MemristorController.Scheduler.Schedule(
                                new AD2Instruction(AD2Instructions.ReadSingle, memristorId, 2));
                        }
                    }

                    timePassed += Time.deltaTime;
                }
                else
                {
                    if (!hasPhase3Completed)
                    {
                        //Start reading pulse every x second
                        if (timePassed > frequency)
                        {
                            if (n >= N)
                            {
                                hasPhase3Completed = true;
                                ExperimentManager.status = ExperimentStatus.NotReady;
                            }
                            else
                            {
                                n++;
                                timePassed = 0;
                                MemristorController.Scheduler.Schedule(
                                    new AD2Instruction(AD2Instructions.ReadSingle, memristorId, 0));
                            }
                        }

                        timePassed += Time.deltaTime;
                    }
                }

            }


            
        }
    }

    private void InitPhase2()
    {
        SetState(PulseUtility.PulseType.WriteState2);
        MemristorController.Stopwatch.Restart();
        timePassed = 0;
        n = 0;
        GraphLineId = 2;
    }

    private void InitPhase3()
    {
        SetState(PulseUtility.PulseType.Erase);
        MemristorController.Stopwatch.Restart();
        timePassed = 0;
        n = 0;
        GraphLineId = 0;
    }

   
}
