using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    List<Button> buttons = new List<Button>();

    [Range(3, 9)]
    public int maxTritValue;
    
    public void Awake()
    {
        //add buttons to list, assume same order as in UI
        var allButtons = gameObject.GetComponentsInChildren<Button>();

        for (int i = 0; i < allButtons.Length; i++)
        {
            buttons.Add(allButtons[i]);
        }

        //init Serial connection
        SerialController.Init();
        MemristorController.Init();
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

        //send to memristor
        MemristorController.Send(id, buttonText.text);
    }

    public void OnToggle(bool status)
    {
        MemristorController.ToggleMemristor(0, status);
    }

    public void OnToggle5(bool status)
    {
        MemristorController.ToggleMemristor(5, status);
    }

    public void OnToggle15(bool status)
    {
        MemristorController.ToggleMemristor(15, status);
    }

    public void Test()
    {
        MemristorController.CheckerboardExperiment();
    }

    public void Reset()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text ="0";
        }
    }

    private void OnApplicationQuit()
    {
        SerialController.Close();
        MemristorController.Close();
    }
}
