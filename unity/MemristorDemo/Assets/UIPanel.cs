using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ExperimentManager;

public class UIPanel : MonoBehaviour
{
    public Experiments experimentPanel;

    public static LineGraphContinuous2D activeGraph; //aaah we can do so much better. We should make a proper UI class per experiment
    
    // Start is called before the first frame update
    void Awake()
    {
        UIManager.Panels.Add(experimentPanel, this.gameObject);
    }

    public void Show()
    {
        //get Panel object containing UI
        transform.GetChild(0).gameObject.SetActive(true);

        //maybe refactor to have everything related to 1 experiment in 1 folder with 3 files (MVC)?
        switch (experimentPanel)
        {
            case Experiments.DC:
                {
                    //init DC graphs, refactor in experiment classes with UI and Logic part
                    activeGraph = UIManager.Panels[Experiments.DC].GetComponentInChildren<LineGraphContinuous2D>();

                    string[] lineLabels = new string[16];
                    for (int i = 0; i < lineLabels.Length; i++)
                    {
                        lineLabels[i] = "Memristor " + i;
                    }

                    activeGraph.Init("DC Experiment", "", "Voltage (V)", "Current (µA)", lineLabels);

                    //TEST GRAPH
                    //graph.AddDataPointToLine(5, new Vector2(-2f, -50f)); //voltage, current
                    //graph.AddDataPointToLine(5, new Vector2(0f, 0f)); //voltage, current
                    //graph.AddDataPointToLine(5, new Vector2(2f, 120f)); //voltage, current
                    //graph.AddDataPointToLine(5, new Vector2(-2f, 120f)); //voltage, current
                }
                break;
            case Experiments.Retention:
            {
                //init DC graphs, refactor in experiment classes with UI and Logic part
                activeGraph = UIManager.Panels[Experiments.Retention].GetComponentInChildren<LineGraphContinuous2D>();

                string[] lineLabels = new string[3];
                lineLabels[0] = "State 0";
                lineLabels[1] = "State 1";
                lineLabels[2] = "State 2";

                var retExp = GameObject.FindObjectOfType<RetentionExperiment>();

                switch (retExp.ReadingIntervalInSec)
                {
                        case RetentionExperiment.Intervals.Freq1Hz:
                            activeGraph.Init("Retention Experiment", "1 Hz interval", "Time (S)", "Resistance (kΩ)", lineLabels);
                            break;
                        case RetentionExperiment.Intervals.Freq1_60Hz:
                            activeGraph.Init("Retention Experiment", "1/60 Hz interval", "Time (M)", "Resistance (kΩ)", lineLabels);
                            break;
                }
           

                    //TEST GRAPH
                    //graph.AddDataPointToLine(5, new Vector2(-2f, -50f)); //voltage, current
                    //graph.AddDataPointToLine(5, new Vector2(0f, 0f)); //voltage, current
                    //graph.AddDataPointToLine(5, new Vector2(2f, 120f)); //voltage, current
                    //graph.AddDataPointToLine(5, new Vector2(-2f, 120f)); //voltage, current
                }
                break;
            default:
                break;
        }
       
    }

    public void Hide()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
