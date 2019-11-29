
using System.Collections.Generic;
//ported to C# from Java
//author: aiunderstand
//based on https://github.com/knowm/memristor-discovery/blob/e414f89b15aeba3ef2a6d21965b071162f6f3189/src/main/java/org/knowm/memristor/discovery/core/rc_engine/RC_ResistanceComputer.java

/**
* used to measure memristor resistance at low currents, when pulse capture results in capacitive
* charge/discharge instead of steady-state voltage divider. Given board series resistor and
* parasitic capacitance, will return the estimated resistance given the read square pulse trace.
*
* @author alexnugent
*/
public class RC_ResistanceComputer
{
    private double parasiticCapacitance;
    private double seriesResistor;
    private double readPulseAmplitude;
    private double readPulseWidth;

    private double[] voltage;
    private double[] resistance;
   
    //  public static void main(String[] args) {
    //
    //    RC_ResistanceComputer rc = new RC_ResistanceComputer(.1, 25E-6, 50_000, 140E-12);
    //    rc.loadTrace();
    //
    //    long startTime = System.currentTimeMillis();
    //
    //    System.out.println(".003-->" + rc.getRFromV(.003));
    //    System.out.println("search time = " + (System.currentTimeMillis() - startTime));
    //
    //  }

    //public RC_ResistanceComputer(double readPulseAmplitude,double readPulseWidth,double seriesResistance,double parasiticCapacitance)
    //{
    //    this.parasiticCapacitance = parasiticCapacitance;
    //    this.seriesResistor = seriesResistance;
    //    this.readPulseAmplitude = readPulseAmplitude;
    //    this.readPulseWidth = readPulseWidth;
        
    //    loadTrace();
    //}

    //public double GetRFromV(double v)
    //{
    //    for (int i = 0; i < voltage.Length; i++)
    //    {

    //            //        System.out.println(
    //            //            "voltage[i]=" + voltage[i] + ", resistance[i]=" + resistance[i] + ", v=" + v);

    //            if (voltage[i] <= v)
    //            {
    //                if (i == 0)
    //                { // edge case
    //                    return resistance[0];
    //                }
    //                // linear interpolation between i and i-1.
    //                double dv = voltage[i - 1] - voltage[i];
    //                double r = (voltage[i - 1] - v) / dv;
    //                double dR = (resistance[i] - resistance[i - 1]) * r;
    //                double interpolation = resistance[i - 1] + dR;
    //                return interpolation;
    //            }
    //    }

    //    return resistance[resistance.Length - 1];
    //}

    //public void loadTrace()
    //{
    //    double Rinit = 1E2;
    //    double Rfinal = 1E8;

    //    List<double> voltage = new List<double>();
    //    List<double> resistance = new List<double>();

    //    double simStepSize = readPulseWidth / 20;
    //    TransientConfig transientConfig = new TransientConfig("" + readPulseWidth, "" + simStepSize, new DC("V1", readPulseAmplitude));

    //    for (double Rm = Rinit; Rm < Rfinal; Rm *= 1.025)
    //    {
    //        Netlist netlist;
    //        netlist = new MD_V2_Board(Rm, seriesResistor, parasiticCapacitance);
         
    //        netlist.setSimulationConfig(transientConfig);
    //        SimulationResult simulationResult = JSpice.simulate(netlist);
    //        SimulationPlotData simulationData = simulationResult.getSimulationPlotDataMap().get("V(2)");
    //        voltage.Add(simulationData.getyData().get(simulationData.getyData().size() - 1));
    //        resistance.Add(Rm);
    //    }

    //    this.voltage = new double[voltage.Count];
    //    this.resistance = new double[resistance.Count];

    //    for (var i = 0; i < this.resistance.Length; i++)
    //    {
    //        this.voltage[i] = voltage[i];
    //        this.resistance[i] = resistance[i];
    //    }
    //}
}