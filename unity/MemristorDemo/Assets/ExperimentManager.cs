﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class ExperimentManager : MonoBehaviour
{
    public enum Experiments
    {
        Checkerboard = 0,
        EraseWriteRead = 1,
        ReadAfterDisconnect = 2,
        DC = 3,
        ADC = 4
    }

    public TMP_Dropdown ExperimentSelector;
    public bool simulateControllers = true;
    public void Awake()
    {
        if (!simulateControllers)
        {
            SerialController.Init();
            MemristorController.Init();
        }
    }

    public void StartExperiment()
    {
        //read dropdown
        var experiment = (Experiments) ExperimentSelector.value;

        switch (experiment)
        {
            case Experiments.Checkerboard:
                MemristorController.CheckerboardExperiment();
                break;
            case Experiments.EraseWriteRead:
                MemristorController.EraseWriteReadTestExperiment();
                break;
            case Experiments.ReadAfterDisconnect:
                MemristorController.ReadTestAfterDisconnectExperiment();
                break;
            case Experiments.DC:
                MemristorController.DCExperiment();
                break;
            case Experiments.ADC:
                MemristorController.OneTritADCExperiment();
                break;
        }
    }

    private void OnApplicationQuit()
    {
        if (!simulateControllers)
        {
            SerialController.Close();
            MemristorController.Close();
        }
    }
}
