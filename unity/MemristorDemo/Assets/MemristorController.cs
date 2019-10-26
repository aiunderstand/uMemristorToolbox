using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WaveFormsSDK;

public class MemristorController
{
    public static ConcurrentQueue<string> Output = new ConcurrentQueue<string>();
    

    public static void Init()
    {
       
    }
    
    public static void Read(int id)
    {



        //put result in Output queue


    }

    public static void Send(int id, string message)
    {
        //check with output if needing change before sending



    }

}
