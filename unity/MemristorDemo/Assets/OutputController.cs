using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SerialController;
using UnityEngine.UI.Extensions;
using static AD2Scheduler;

public class OutputController : MonoBehaviour
{
    public RangeSlider RangeSlider;    

    List<Button> memristors = new List<Button>();
    float delayTime = 0;
    float delay = 0.05f;

    [Range (0.5f,10f)]
    public float ReadingIntervalInSec = 10; //10 Seconds intervals
    float timePassed = 0; 

    public static int UpperLimitState = 100; //refactor this should the range UI slider is used by more experiments
    public static int LowerLimitState = 8;


    public void Awake()
    {
        //add buttons to list, assume same order as in UI
        var allLeds = gameObject.GetComponentsInChildren<Button>();

        for (int i = 0; i < allLeds.Length; i++)
        {
            memristors.Add(allLeds[i]);
        }

        //DEBUG
        //MemristorController.Output.Enqueue("1,2");        
        //MemristorController.Output.Enqueue("16,1");
        MemristorController.Scheduler.Clear();

        OutputController.LowerLimitState = (int) RangeSlider.LowValue;
        OutputController.UpperLimitState = (int) RangeSlider.HighValue;
    }

    public void OnButtonPressed(int id)
    {
        //get selected button, -1 for index start at 1
        var button = memristors[id - 1];

        //get textcomponent of button
        var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();


        //toggle enabled state by putting a - sign in label
        if (buttonText.text.Equals("-"))
        {
            //ENABLE memristor
            //send a read signal to memristor, because it was unknown
            MemristorController.ToggleMemristor(id, true);
            MemristorController.Scheduler.Schedule(new AD2Instruction(AD2Instructions.ReadSingle, id));            
        }
        else
        {
            //DISABLE memristor
            //send a clear signal to output LEDD only (not send a erase to memristor)
            MemristorController.ToggleMemristor(id, false);
            MemristorController.Output.Enqueue(string.Format("{0},-1", id.ToString()));
        }
    } 

    public void Update()
    {
        if (ExperimentManager.status == ExperimentManager.ExperimentStatus.Started)
        {
            if (delayTime >= delay)
            {
                delayTime = 0;

                //check Output queue, if something process one per frame
                if (MemristorController.Output.Count > 0)
                {
                    string message = "";
                    MemristorController.Output.TryDequeue(out message);

                    if (!message.Equals(""))
                    {
                        //message type is of string format "id,value"  
                        var parts = message.Split(',');
                        var id = int.Parse(parts[0]);
                        var value = parts[1];

                        //update Memristor Output UI
                        var led = memristors[id - 1].GetComponentInChildren<TextMeshProUGUI>();

                        //enable or disable using memristor
                        if (value.Contains("-1")) //DISABLE LED
                        {
                            led.text = "-";
                            value = "0";

                            //update Hardware LEDs
                            SerialController.Send(string.Format("${0},{1},{2};", (int) MessageType.UpdateMatrixSingle,
                                id, value));
                        }
                        else //ENABLE LED
                        {
                            led.text = value;

                            //update Hardware LEDs
                            SerialController.Send(string.Format("${0},{1},{2};", (int) MessageType.UpdateMatrixSingle,
                                id, value));
                        }
                    }
                }
            }

            //Start reading pulse every x second
            if (timePassed > ReadingIntervalInSec)
            {
                timePassed = 0;
                MemristorController.Scheduler.Schedule(new AD2Instruction(AD2Instructions.ReadSingle, 1));
            }

            delayTime += Time.deltaTime;
            timePassed += Time.deltaTime;
        }
    }


}
