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
package org.knowm.jspice.circuits;

import org.knowm.jspice.component.element.reactive.Capacitor;
import org.knowm.jspice.netlist.Netlist;
import org.knowm.jspice.netlist.NetlistCapacitor;
import org.knowm.jspice.netlist.NetlistDCVoltage;
import org.knowm.jspice.netlist.NetlistResistor;

public class V1R1C1 extends Netlist {

  public V1R1C1() {

    //    // define capacitor
    //    Capacitor capacitor1 = new Capacitor("C1", 16E-8);
    //    capacitor1.setInitialCondition(0);

    // build netlist, the nodes can be named anything except for ground whose node is always labeled "0"
    addNetListComponent(new NetlistDCVoltage("V1", 0.0, "1", "0"));
    addNetListComponent(new NetlistResistor("R1", 160000, "1", "2"));
    addNetListComponent(new NetlistCapacitor("C1", 16E-8, "2", "0"));
  }
}
