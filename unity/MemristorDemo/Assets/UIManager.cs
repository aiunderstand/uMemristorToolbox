using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ExperimentManager;

public class UIManager : Singleton<UIManager>
{
    public static Dictionary<Experiments, GameObject> Panels = new Dictionary<Experiments, GameObject>();

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
