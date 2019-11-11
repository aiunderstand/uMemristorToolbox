//based on https://github.com/knowm/memristor-discovery/blob/e414f89b15aeba3ef2a6d21965b071162f6f3189/src/main/java/org/knowm/memristor/discovery/gui/mvc/experiments/pulse/control/ControlModel.java
//REFACTOR default settings as part of model and different settings files per experiment
//settings panel https://github.com/knowm/memristor-discovery/blob/e414f89b15aeba3ef2a6d21965b071162f6f3189/src/main/java/org/knowm/memristor/discovery/gui/mvc/experiments/dc/DCPreferencesPanel.java
//example file https://github.com/knowm/memristor-discovery/blob/e414f89b15aeba3ef2a6d21965b071162f6f3189/src/main/java/org/knowm/memristor/discovery/gui/mvc/experiments/dc/DCPreferences.java
//helpers based on: https://github.com/knowm/memristor-discovery/blob/e414f89b15aeba3ef2a6d21965b071162f6f3189/src/main/java/org/knowm/memristor/discovery/gui/mvc/experiments/ExperimentPreferences.java
using static TimeUnitsHelper;
using static ConductanceUnitsHelper;
using static CurrentUnitsHelper;
using static ResistanceUnitsHelper;
public class MemristorModel
{
    

    private int pulseWidth = 50_000; // model store pulse width in nanoseconds
    private double dutyCycle = .5f; // 0 to 1 range.
    private float amplitude = 1f;
    private int pulseNumber = 1;
    private int period; 

    private MemristorController.Waveform waveform = MemristorController.Waveform.TriangleUpDown;
    public TimeUnits TIME_UNIT = TimeUnits.MilliSeconds;
    public CurrentUnits CURRENT_UNIT = CurrentUnits.MicroAmps;
    public ConductanceUnits CONDUCTANCE_UNIT = ConductanceUnits.MilliSiemens;
    public ResistanceUnits RESISTANCE_UNIT = ResistanceUnits.KiloOhms;



    //https://github.com/knowm/memristor-discovery/blob/e414f89b15aeba3ef2a6d21965b071162f6f3189/src/main/java/org/knowm/memristor/discovery/gui/mvc/experiments/pulse/control/ControlModel.java
    public double GetCalculatedFrequencyPulse()
    {
        return (1.0 / (pulseWidth / dutyCycle) * 1_000_000_000); // 50% duty cycle
    }

    //https://github.com/knowm/memristor-discovery/blob/e414f89b15aeba3ef2a6d21965b071162f6f3189/src/main/java/org/knowm/memristor/discovery/gui/mvc/experiments/dc/control/ControlModel.java
    public double GetCalculatedFrequencyDC()
    {
        return 1.0 / ((double)period) * TimeUnitsHelper.GetDivisor(TIME_UNIT); // 50% duty cycle
    }

    public double GetAmplitude()
    {
        return amplitude;
    }

    public int GetPulseNumber()
    {

        return pulseNumber;
    }

    public int GetPeriod()
    {

        return period;
    }

    public MemristorController.Waveform GetWaveform()
    {
        return waveform;
    }

    public void SetWaveform(MemristorController.Waveform selectedWaveform)
    {
        waveform = selectedWaveform;
    }

    public void SetPulseNumber(int number)
    {
        pulseNumber = number;
    }

    public void SetAmplitude(float selectedAmplitude)
    {
        amplitude = selectedAmplitude;
    }

    public void SetPeriod(int selectedPeriod)
    {
        period = selectedPeriod;
    }
}

public class TimeUnitsHelper
{
    public enum TimeUnits
    {
        Seconds= 1,
        MilliSeconds =1000,
        MicroSeconds =1_000_000
    }
    
    public static double GetDivisor(TimeUnits unit)
    {
        return (double) unit;
    }

    public static string GetLabel(TimeUnits unit)
    {
        switch (unit)
        {
            case TimeUnits.Seconds:
                return "s";
            case TimeUnits.MilliSeconds:
                return "ms";
            case TimeUnits.MicroSeconds:
                return "µs";
            default:
                return "?";
        }
    }
}

public class CurrentUnitsHelper
{
    public enum CurrentUnits
    {
        Amps = 1,
        Milliamps = 1000,
        MicroAmps = 1_000_000
    }

    public static double GetDivisor(CurrentUnits unit)
    {
        return (double)unit;
    }

    public static string GetLabel(CurrentUnits unit)
    {
        switch (unit)
        {
            case CurrentUnits.Amps:
                return "A";
            case CurrentUnits.Milliamps:
                return "mA";
            case CurrentUnits.MicroAmps:
                return "µA";
            default:
                return "?";
        }
    }
}

public class ResistanceUnitsHelper
{
    public enum ResistanceUnits
    {
        Ohms = 1,
        KiloOhms = 1000,
        MegaOhms = 1_000_000
    }

    public static double GetDivisor(ResistanceUnits unit)
    {
        return (double)unit;
    }

    public static string GetLabel(ResistanceUnits unit)
    {
        switch (unit)
        {
            case ResistanceUnits.Ohms:
                return "Ω";
            case ResistanceUnits.KiloOhms:
                return "kΩ";
            case ResistanceUnits.MegaOhms:
                return "mΩ";
            default:
                return "?";
        }
    }
}

public class ConductanceUnitsHelper
{
    public enum ConductanceUnits
    {
        Siemens = 1,
        MilliSiemens = 1000,
        MicroSiemens = 1_000_000
    }
   
    public static double GetDivisor(ConductanceUnits unit)
    {
        return (double)unit;
    }

    public static string GetLabel(ConductanceUnits unit)
    {
        switch (unit)
        {
            case ConductanceUnits.Siemens:
                return "S";
            case ConductanceUnits.MilliSiemens:
                return "mS";
            case ConductanceUnits.MicroSiemens:
                return "µS";
            default:
                return "?";
        }
    }
}

public class PostProcessHelper
{
    public static double[] Invert(double[] v)
    {
        double[] vminus = new double[v.Length];
        for (int i = 0; i < v.Length; i++)
        {
            vminus[i] = -v[i];
        }
        return vminus;
    }
}