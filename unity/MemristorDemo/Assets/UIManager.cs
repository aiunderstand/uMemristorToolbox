using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ExperimentManager;

public class UIManager : Singleton<UIManager>
{
    public static Dictionary<Experiments, GameObject> Panels = new Dictionary<Experiments, GameObject>();

    public void Start()
    {
        //init DC graphs, refactor in experiment classes with UI and Logic part
        var graph = Panels[Experiments.DC].GetComponentInChildren<LineGraphContinuous2D>();

        string[] lineLabels = new string[16];
        for (int i = 0; i < lineLabels.Length; i++)
        {
            lineLabels[i] = "Memristor " + i;
        }

        graph.Init("DC Experiment", "", "Voltage (V)", "Current (µA)", lineLabels);

        graph.AddDataPointToLine(5, new Vector2(-2f, -50f)); //voltage, current
        graph.AddDataPointToLine(5, new Vector2(0f, 0f)); //voltage, current
        graph.AddDataPointToLine(5, new Vector2(2f, 120f)); //voltage, current
        graph.AddDataPointToLine(5, new Vector2(-2f, 120f)); //voltage, current
    }

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
}
