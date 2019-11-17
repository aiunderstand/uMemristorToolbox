using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AD2Scheduler;

public class InputController : MonoBehaviour
{
    public List<Button> buttons = new List<Button>();
    public static List<int> GroundTruth =new List<int>();

    int maxTritValue =3;

    public void Awake()
    {
        //add buttons to list, assume same order as in UI
        var allButtons = gameObject.GetComponentsInChildren<Button>();

        for (int i = 0; i < allButtons.Length; i++)
        {
            buttons.Add(allButtons[i]);
            GroundTruth.Add(0);
        }

    }

    public void Update()
    {
        //check RX queue, if something process all
        for (int i = 0; i < SerialController.RX.Count; i++)
        {
            string message ="";
            SerialController.RX.TryDequeue(out message);

            if (!message.Equals(""))
            {
                if (message.Equals("reset"))
                {
                    Reset();
                }
                else
                {
                    //parse message, using string format "id"
                    var id = int.Parse(message);

                    OnButtonPressed(id);
                }
            }
        }
        
    }

    public void OnButtonPressed(int id)
    {
        //get selected button, -1 for index start at 1
        var button = buttons[id-1];

        //get textcomponent of button
        var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

        //update value
        int value = int.Parse(buttonText.text) +1;
        value = value % maxTritValue;

        //set value
        buttonText.text = value.ToString();
        GroundTruth[id - 1] = value;

        //send to memristor
        MemristorController.Scheduler.Schedule(new AD2Instruction(AD2Instructions.WriteSingle,id, value));
    }
      
    public void Reset()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text ="0";
        }
    }
}
