using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public static string logPath = @"C:\Users\maxde\Google Drive\USN PhD Robotics\Data\Memristor.txt";
    public static bool outputToConsole = true;
    public static List<string> dataQueue = new List<string>();

    public static void SaveExperimentDataToLog() {
        dataQueue.Add(string.Format("Experiment ended. Duration: {0}hh:{1}mm:{2}ss", MemristorController.Stopwatch.Elapsed.Hours, MemristorController.Stopwatch.Elapsed.Minutes, MemristorController.Stopwatch.Elapsed.Seconds));

        StreamWriter writer = new StreamWriter(logPath, true);

        var strings = dataQueue.ToArray();

        foreach (var s in strings)
        {
            if (outputToConsole)
                Debug.Log(s);

            writer.WriteLine(s);
        }

        //empty line
        if (outputToConsole)
            Debug.Log("");

        writer.WriteLine("");

        writer.Close();
        dataQueue.Clear();
    }
}
