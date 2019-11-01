using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    List<Button> buttons = new List<Button>();

    int maxTritValue =3;

    public TMP_Dropdown experimentSelector;

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

    public void StartExperiment()
    {
        //read dropdown
        var index = experimentSelector.value;

        switch (index)
        {
            case 0:
                MemristorController.CheckerboardExperiment();
                break;
            case 1:
                MemristorController.ReadTestExperiment();
                break;
            case 2:
                MemristorController.ReadTestAfterDisconnectExperiment();
                break;
        }
        
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
