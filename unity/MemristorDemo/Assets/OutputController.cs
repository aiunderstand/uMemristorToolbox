using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SerialController;

public class OutputController : MonoBehaviour
{
    List<Button> memristors = new List<Button>();
    float delayTime = 0;
    float delay = 0.05f;


    public void Awake()
    {
        //add buttons to list, assume same order as in UI
        var allLeds = gameObject.GetComponentsInChildren<Button>();

        for (int i = 0; i < allLeds.Length; i++)
        {
            memristors.Add(allLeds[i]);
        }
    }

    public void Start()
    {
        //MemristorController.Output.Enqueue("1,2");
        //MemristorController.Output.Enqueue("2,2");
        //MemristorController.Output.Enqueue("3,2");
        //MemristorController.Output.Enqueue("4,2");

        //MemristorController.Output.Enqueue("5,1");
        //MemristorController.Output.Enqueue("6,1");
        //MemristorController.Output.Enqueue("7,1");
        //MemristorController.Output.Enqueue("8,1");

        //MemristorController.Output.Enqueue("9,2");
        //MemristorController.Output.Enqueue("10,2");
        //MemristorController.Output.Enqueue("11,2");
        //MemristorController.Output.Enqueue("12,2");

        //MemristorController.Output.Enqueue("13,1");
        //MemristorController.Output.Enqueue("14,1");
        //MemristorController.Output.Enqueue("15,1");
        //MemristorController.Output.Enqueue("16,1");
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
            //send a clear signal to memristor
            MemristorController.ToggleMemristor(id, true);
            //MemristorController.Read(id, buttonText.text);            
        }
        else
        {
            MemristorController.ToggleMemristor(id, false);
            MemristorController.Output.Enqueue(string.Format("{0},-1", id.ToString()));
        }

        
    } 

    public void Update()
    {
        delayTime += Time.deltaTime;
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
                    if (!value.Contains("-1"))
                    {
                        led.text = value;

                        //update Hardware LEDs
                        SerialController.Send(string.Format("${0},{1},{2};", (int)MessageType.UpdateMatrixSingle, id, value));
                    }
                    else
                    {
                        led.text = "-";
                        value = "0";

                        //update Hardware LEDs
                        SerialController.Send(string.Format("${0},{1},{2};", (int)MessageType.UpdateMatrixSingle, id, value));
                    }
                }
            }
        }
    }


}
