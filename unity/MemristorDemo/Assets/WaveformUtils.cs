/* THIS CODE HAS BEEN PORTED AND ADAPTED FROM https://raw.githubusercontent.com/knowm/memristor-discovery/develop/src/main/java/org/knowm/memristor/discovery/core/experiment_common/PulseUtility.java
 * Author: AIUNDERSTAND  
 
/**
 * Memristor-Discovery is distributed under the GNU General Public License version 3 and is also
 * available under alternative licenses negotiated directly with Knowm, Inc.
 *
 * <p>Copyright (c) 2016-2019 Knowm Inc. www.knowm.org
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


using static MemristorController;
/** Created by timmolter on 2/15/17. */
public class WaveformUtils
{
    public static double[] generateCustomWaveform(Waveform waveform, double amplitude, double frequency)
    {
        Driver driver;
        switch (waveform)
        {
            case Waveform.Square:
                driver = new Square("Square", amplitude / 2, 0, amplitude / 2, frequency);
                break;
            case Waveform.HalfSine:
                driver = new HalfSine("HalfSine", 0, 0, amplitude, frequency);
                break;
            case Waveform.TriangleUpDown:
                driver = new TriangleUpDown("TriangleUpDown", 0, 0, amplitude, frequency);
                break;
            default:
                driver = new Square("Square", amplitude / 2, 0, amplitude / 2, frequency); //default was sawtooth!
                break;
        }

        int counter = 0;
        double[] customWaveform = new double[4096];
        double timeInc = 1.0 / frequency / 4096;

        do
        {
            double time = counter * timeInc;
            customWaveform[counter] = driver.getSignal(time) / 5.0; // / 5.0 to scale between 1 and -1
        } while (++counter < 4096);

        return customWaveform;
    }
}