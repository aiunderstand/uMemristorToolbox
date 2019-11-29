using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using static ExperimentManager;

public class UIManager : Singleton<UIManager>
{
    public static Dictionary<Experiments, GameObject> Panels = new Dictionary<Experiments, GameObject>();
    public static ConcurrentQueue<GraphInstruction> GraphQueue = new ConcurrentQueue<GraphInstruction>();
    
    public void ShowExperimentPanel(int experimentId)
    {
        foreach (var p in Panels)
        {
            if (p.Key.Equals((Experiments) experimentId))
                p.Value.GetComponent<UIPanel>().Show();
            else
                p.Value.GetComponent<UIPanel>().Hide();
        }
    }

    public void Update()
    {
        if (GraphQueue.Count > 0)
        {

            GraphInstruction instruction;
            GraphQueue.TryDequeue(out instruction);

            if (instruction != null)
            {
                switch (instruction.Experiment)
                {
                    case Experiments.Retention:
                    {
                            UIPanel.activeGraph.AddConductanceTimeDataPointToLine(RetentionExperiment.GraphLineId,instruction.Data);
                    }
                        break;
                }
            }
        }

    }
}

public class GraphInstruction
{
    public Experiments Experiment;
    public Vector2 Data;

    public GraphInstruction(Experiments experiments, Vector2 data)
    {
        Experiment = experiments;
        Data = data;
    }
}