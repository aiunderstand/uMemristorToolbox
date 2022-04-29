using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static AD2Scheduler;
using static MemristorController;

public class RandomWriteExperiment : MonoBehaviour
{
    public int N = 5; //amount of numbers in each state
    private List<int> groundTruthStates = new List<int>();
    private List<int> actualStates = new List<int>();
    private ConfusionMatrix cm;
    private bool once = true;

    [Range(1, 16)]
    public int memristorId = 1;

    public void StartExperiment()
    {
        cm = gameObject.GetComponentInChildren<ConfusionMatrix>();

        var date = DateTime.Today.ToShortDateString();
        var time = DateTime.Now.ToShortTimeString();
        //this should be refactored in the logger class
        MemristorController.Stopwatch.Start();

      
        //HEADER
        var experiment = "RandomWrite Experiment";
        var settings = string.Format("DATE: {0}  TIME: {5} V_WRITE: {1}v  V_RESET: {2}v  V_READ {3}v  SERIES_RES {4}Ω  MemristorId {5}  N {6}", date, V_WRITE, V_RESET, V_READ, SERIES_RESISTANCE, time, memristorId, N);
        Logger.dataQueue.Add(experiment);
        Logger.dataQueue.Add(settings);

        //start with 2 erases 
        PulseUtility.EraseSingleMemristor(memristorId-1, Waveform.HalfSine, -V_RESET, PULSE_WIDTH_IN_MICRO_SECONDS);
        
        //Create random dataset of x 0's,x 1's, x 2's numbers
        List<int> nonRandomList = new List<int>();
        for (int i = 0; i < N*3; i++)
        {
            var state = i % 3;
            nonRandomList.Add(state);
        }

        //Randomize states;
        System.Random r = new System.Random();
        int randomIndex = 0;
        while (nonRandomList.Count > 0)
        {
            randomIndex = r.Next(0, nonRandomList.Count); //Choose a random object in the list
            groundTruthStates.Add(nonRandomList[randomIndex]); //add it to the new, random list
            nonRandomList.RemoveAt(randomIndex); //remove to avoid duplicates
        }

        //add all writes to schedule
        for (int i = 0; i < groundTruthStates.Count; i++)
        {
            MemristorController.Scheduler.Schedule(new AD2Instruction(AD2Instructions.WriteSingle, memristorId, groundTruthStates[i], 0, 0, MemristorController.Waveform.HalfSine, 0)); //we ignore the part after value
        }

        MemristorController.Stopwatch.Restart(); //this is not accurate enough at this position.
        StartScheduler();
    }

    public void Update()
    {
        if (ExperimentManager.experiment == ExperimentManager.Experiments.RandomWrite)
        {
            //we only need this update to poll if experiment ended. This can be refactored to somehting better
            //check Output queue, if something process one per frame
            if (MemristorController.Output.Count > 0)
            {
                string result = "";
                MemristorController.Output.TryDequeue(out result);
                var parts = result.Split(',');
                actualStates.Add(int.Parse(parts[1]));
            }

            if ((actualStates.Count == (N * 3)) && once)
            {
                cm.UpdateMatrix(groundTruthStates, actualStates);
                once = false;
                Scheduler.IsActive = false;
            }
        }
    }
}
