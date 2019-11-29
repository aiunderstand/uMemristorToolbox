//using Cyotek.Collections.Generic;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using UnityEngine;

//public class Dataplotter : MonoBehaviour
//{
//    public GameObject Graph1;
//    public GameObject Graph2;
//    public GameObject Graph3;
//    public GameObject Graph4;

//    CircularBuffer<double> graph1BufferLoadPosition = new CircularBuffer<double>(180, true);
//    CircularBuffer<double> graph1BufferLoadVelocity = new CircularBuffer<double>(180, true);
//    CircularBuffer<double> graph2BufferMotorPosition = new CircularBuffer<double>(180, true);
//    CircularBuffer<double> graph2BufferMotorVelocity = new CircularBuffer<double>(180, true);
//    CircularBuffer<double> graph3Buffer = new CircularBuffer<double>(180, true);
//    CircularBuffer<double> graph4Buffer = new CircularBuffer<double>(180, true);
//    Graph _graph1;
//    Graph _graph2;
//    Graph _graph3;
//    Graph _graph4;

//    double t = 0;
//    StreamWriter w;
//    ElasticActuatorController controller;
//    float timeElapsed = 0;
//    Stopwatch sw = new Stopwatch();
//    int frameId=0;

//    private void Start()
//    {
//        _graph1 = Graph1.GetComponent<Graph>();
//        _graph1.Init("Load", "Time (in sec) DeltaT = 0.0001s, Plotrate = 1:1000", "Angles (in radians)", "position", "velocity");
//        _graph1.AttachTo(graph1BufferLoadPosition, 0);
//        _graph1.AttachTo(graph1BufferLoadVelocity, 1);

//        _graph2 = Graph2.GetComponent<Graph>();
//        _graph2.Init("Motor", "Time (in sec) DeltaT = 0.0001s, Plotrate = 1:1000", "Angles (in radians)", "position","velocity");
//        _graph2.AttachTo(graph2BufferMotorPosition, 0);
//        _graph2.AttachTo(graph2BufferMotorVelocity, 1);

//        _graph3 = Graph3.GetComponent<Graph>();
//        _graph3.Init("Torque", "Time (in sec) DeltaT = 0.0001s, Plotrate = 1:1000", "Torque (in Nm)");
//        _graph3.AttachTo(graph3Buffer, 0);

//        _graph4 = Graph4.GetComponent<Graph>();
//        _graph4.Init("Error", "Time (in sec) DeltaT = 0.0001s, Plotrate = 1:1000", "Error (in radians)");
//        _graph4.AttachTo(graph4Buffer, 0);

//        //open streamwriter
//        w = new StreamWriter(Application.dataPath + "\\data.csv", append: true);

//        //get a reference to the controller
//        controller = GameObject.FindObjectOfType<ElasticActuatorController>();
        
//    }

//    public void FixedUpdate()
//    {
//        //if (frameId == 0)
//        //    sw.Start();

//        //if (frameId % 100000 == 0)
//        //    UnityEngine.Debug.Log("frame: " + frameId + " t_sim in sumdeltaT: " + t + " T_sim in sec: " + (Time.realtimeSinceStartup) + " T_actual in sec: " + sw.Elapsed.TotalSeconds);

//        if (controller.useDataPlotter)
//        {
//            timeElapsed += Time.fixedDeltaTime;
//            //collect all data (2do:should do this with decorators)
//            if (timeElapsed > ElasticActuatorController.DataPlotterRate)
//            {
                
//                timeElapsed = 0;

//                graph1BufferLoadPosition.Put(ElasticActuatorController.SampleThetaLoadActual);
//                graph1BufferLoadVelocity.Put(ElasticActuatorController.SampleThetaLoadActualDot);
//                graph2BufferMotorPosition.Put(ElasticActuatorController.SampleThetaMotorActual);
//                graph2BufferMotorVelocity.Put(ElasticActuatorController.SampleThetaMotorActualDot);

//                graph3Buffer.Put(ElasticActuatorController.SampleTauMotorDesired);
//                graph4Buffer.Put(ElasticActuatorController.SampleEpsilon);

//                //plot the data
//                _graph1.Plot();
//                _graph2.Plot();
//                _graph3.Plot();
//                _graph4.Plot();
//            }

//            if (frameId % 180000 == 0 && frameId !=0)
//                UnityEngine.Debug.Break();
//        }

//        //save the data
//        if (controller.recordSession)
//            Save();

//        frameId++;
      
//        t += Time.fixedDeltaTime;
//    }

//    void Save()
//    {
//       var line = string.Format("{0};{1};{2};",
//            t.ToString(),
//            ElasticActuatorController.SampleThetaLoadActual.ToString(),
//            ElasticActuatorController.SampleThetaLoadActualDot.ToString());

//        w.WriteLine(line);
//        //w.Flush();
//    }

//    private void OnDestroy()
//    {
//        w.Close();
//    }

//}
