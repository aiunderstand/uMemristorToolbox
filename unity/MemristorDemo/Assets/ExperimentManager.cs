using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public enum Experiments
    {
        Checkerboard = 0,
        Read = 1,
        ReadAfterDisconnect = 2,
        DC = 3,
        ADC = 4
    }

    public TMP_Dropdown ExperimentSelector;

    public void Awake()
    {
        SerialController.Init();
        MemristorController.Init();
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
            case Experiments.Read:
                MemristorController.ReadTestExperiment();
                break;
            case Experiments.ReadAfterDisconnect:
                MemristorController.ReadTestAfterDisconnectExperiment();
                break;
            case Experiments.DC:
                MemristorController.DCExperiment();
                break;
            case Experiments.ADC:
                //MemristorController.ADCExperiment();
                break;
        }
    }

    private void OnApplicationQuit()
    {
        SerialController.Close();
        MemristorController.Close();
    }
}
