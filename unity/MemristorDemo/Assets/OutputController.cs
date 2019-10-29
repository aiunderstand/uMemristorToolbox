using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SerialController;

public class OutputController : MonoBehaviour
{
    List<Button> leds = new List<Button>();
    float delayTime = 0;
    float delay = 0.05f;


    public void Awake()
    {
        //add buttons to list, assume same order as in UI
        var allLeds = gameObject.GetComponentsInChildren<Button>();

        for (int i = 0; i < allLeds.Length; i++)
        {
            leds.Add(allLeds[i]);
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
                    var led = leds[id - 1].GetComponentInChildren<TextMeshProUGUI>();
                    led.text = value;

                    //update Hardware LEDs
                    SerialController.Send(string.Format("${0},{1},{2};", (int)MessageType.UpdateMatrixSingle, id, value));
                }
            }
        }
    }


}
