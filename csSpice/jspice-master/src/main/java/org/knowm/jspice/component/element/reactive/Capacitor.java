/**
 * jspice is distributed under the GNU General Public License version 3
 * and is also available under alternative licenses negotiated directly
 * with Knowm, Inc.
 *
 * Copyright (c) 2016-2017 Knowm Inc. www.knowm.org
 *
 * Knowm, Inc. holds copyright
 * and/or sufficient licenses to all components of the jspice
 * package, and therefore can grant, at its sole discretion, the ability
 * for companies, individuals, or organizations to create proprietary or
 * open source (even if not GPL) modules which may be dynamically linked at
 * runtime with the portions of jspice which fall under our
 * copyright/license umbrella, or are distributed under more flexible
 * licenses than GPL.
 *
 * The 'Knowm' name and logos are trademarks owned by Knowm, Inc.
 *
 * If you have any questions regarding our licensing policy, please
 * contact us at `contact@knowm.org`.
 */
package org.knowm.jspice.component.element.reactive;

import java.util.HashSet;
import java.util.Map;
import java.util.Set;

import org.knowm.jspice.netlist.Netlist;
import org.knowm.jspice.simulate.dcoperatingpoint.DCOperatingPointResult;

/**
 * @author timmolter
 */
public class Capacitor extends ReactiveElement {

  private double capacitance;

  /**
   * Constructor
   *
   * @param id
   * @param capacitance
   */
  public Capacitor(String id, double capacitance) {

    super(id);
    this.capacitance = capacitance;
  }

  public Capacitor(String id, double capacitance, double initialCondition) {

    super(id);
    this.capacitance = capacitance;
    setInitialCondition(initialCondition);
  }

  @Override
  public void setSweepValue(double value) {

    this.capacitance = value;
  }

  @Override
  public double getSweepableValue() {

    return capacitance;
  }

  @Override
  public String toString() {

    return "Capacitor [id=" + getId() + ", capacitance=" + capacitance + "]";
  }

  @Override
  public Set<String> getGMatrixColumnIDs(String[] nodes, Double timeStep) {

    // DCVoltage
    Set<String> set = new HashSet<>(3);
    set.add(nodes[0]);
    set.add(nodes[1]);
    if (timeStep != null) { // transient
      set.add(getId()); // either for initial DCVoltage or for transient Current Source
    }

    return set;
  }

  @Override
  public void modifyUnknowmQuantitiesVector(String[] columnQuantities, String[] nodes, Double timeStep) {

    for (int i = 0; i < columnQuantities.length; i++) {
      if (columnQuantities[i].equals(nodes[0]) || columnQuantities[i].equals(nodes[1])) {
        columnQuantities[i] = "V(" + columnQuantities[i] + ")";
      } else if (columnQuantities[i].equals(getId())) {
        if (timeStep != null) { // transient
          columnQuantities[i] = "I(" + columnQuantities[i] + ")";
        }
      }
    }
  }

  @Override
  public void stampG(double[][] G, Netlist netList, DCOperatingPointResult dcOperatingPointResult, Map<String, Integer> nodeID2ColumnIdxMap,
      String[] nodes, Double timeStep) {

    // current source
    // no contribution

    if (timeStep != null) {

      int idxA = nodeID2ColumnIdxMap.get(nodes[0]);
      int idxB = nodeID2ColumnIdxMap.get(nodes[1]);
      int idxI = nodeID2ColumnIdxMap.get(getId());

      // create stamp
      double[][] stamp = new double[3][3];

      stamp[0][0] = 0.0;
      stamp[0][1] = 0.0;
      stamp[0][2] = 1.0;
      stamp[1][0] = 0.0;
      stamp[1][1] = 0.0;
      stamp[1][2] = -1.0;

      double g_eq = capacitance / timeStep;
      stamp[2][0] = g_eq;
      stamp[2][1] = -1.0 * g_eq;
      stamp[2][2] = -1.0;

      // apply stamp
      G[idxA][idxA] += stamp[0][0];
      G[idxA][idxB] += stamp[0][1];
      G[idxA][idxI] += stamp[0][2];
      G[idxB][idxA] += stamp[1][0];
      G[idxB][idxB] += stamp[1][1];
      G[idxB][idxI] += stamp[1][2];
      G[idxI][idxA] += stamp[2][0];
      G[idxI][idxB] += stamp[2][1];
      G[idxI][idxI] += stamp[2][2];
    }
  }

  @Override
  public void stampRHS(double[] RHS, DCOperatingPointResult dcOperatingPointResult, Map<String, Integer> nodeID2ColumnIdxMap, String[] nodes,
      Double timeStep) {

    if (timeStep != null) {

      double voltageA = dcOperatingPointResult.getValue("V(" + nodes[0] + ")");
      double voltageB = dcOperatingPointResult.getValue("V(" + nodes[1] + ")");
      double Vcapacitor = voltageA - voltageB;

      // create stamp
      double[] stamp = new double[3];

      stamp[0] = 0.0;
      stamp[1] = 0.0;
      stamp[2] = capacitance / timeStep * Vcapacitor;

      // apply stamp
      int idxA = nodeID2ColumnIdxMap.get(nodes[0]);
      int idxB = nodeID2ColumnIdxMap.get(nodes[1]);
      int idxI = nodeID2ColumnIdxMap.get(getId());
      RHS[idxA] += stamp[0];
      RHS[idxB] += stamp[1];
      RHS[idxI] += stamp[2];
    }
  }

  @Override
  public void stampSolutionVector(double[] solutionVector, Map<String, Integer> nodeID2ColumnIdxMap, String[] nodes) {

    // System.out.println("nodeID2ColumnIdxMap= " + nodeID2ColumnIdxMap);

    if (getInitialCondition() != null) {
      // System.out.println(nodes[0]);
      int idxA = nodeID2ColumnIdxMap.get(nodes[0]);
      // System.out.println(idxA);
      solutionVector[idxA] = getInitialCondition();
    }
  }
}
