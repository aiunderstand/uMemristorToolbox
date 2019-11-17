﻿/* THIS CODE HAS BEEN PORTED AND ADAPTED FROM https://raw.githubusercontent.com/knowm/memristor-discovery/develop/src/main/java/org/knowm/memristor/discovery/core/experiment_common/PulseUtility.java
 * Author: AIUNDERSTAND  
 
/**
 * Memristor-Discovery is distributed under the GNU General Public License version 3 and is also
 * available under alternative licenses negotiated directly with Knowm, Inc.
 *
 * <p>Copyright (c) 2016-2019 Knowm Inc. www.knowm.org
 *
 * <p>This package also includes various components that are not part of Memristor-Discovery itself:
 *
 * <p>* `Multibit`: Copyright 2011 multibit.org, MIT License * `SteelCheckBox`: Copyright 2012
 * Gerrit, BSD license
 *
 * <p>Knowm, Inc. holds copyright and/or sufficient licenses to all components of the
 * Memristor-Discovery package, and therefore can grant, at its sole discretion, the ability for
 * companies, individuals, or organizations to create proprietary or open source (even if not GPL)
 * modules which may be dynamically linked at runtime with the portions of Memristor-Discovery which
 * fall under our copyright/license umbrella, or are distributed under more flexible licenses than
 * GPL.
 *
 * <p>The 'Knowm' name and logos are trademarks owned by Knowm, Inc.
 *
 * <p>If you have any questions regarding our licensing policy, please contact us at
 * `contact@knowm.org`.
 */

using System;


/** @author timmolter */
public class SquareSmooth : WaveformDriver
{

    /**
* Constructor
*
* @param name
* @param dcOffset
* @param phase
* @param amplitude
* @param frequency
*/
    public SquareSmooth(string name, double dcOffset, double phase, double amplitude, double frequency) : base(name, dcOffset, phase, amplitude, frequency)
    {
    }

    public override double getSignal(double time)
    {
        double T = 1 / frequency;
        double remainderTime = (time + phase) % T;

        // up phase 1
        if (0 <= remainderTime && remainderTime * T < .10 / frequency * T)
        {
            return 10 * frequency * amplitude * (remainderTime) + dcOffset;
        }
        // up phase 2
        else if (0 <= remainderTime && remainderTime * T < .40 / frequency * T)
        {
            return amplitude + dcOffset;
        }
        // up phase 3
        else if (0 <= remainderTime && remainderTime * T < .50 / frequency * T)
        {
            return -10 * frequency * amplitude * (remainderTime) + dcOffset + amplitude * 5;
        }

        // down phase
        else
        {
            return 0.0;
        }
    }
}