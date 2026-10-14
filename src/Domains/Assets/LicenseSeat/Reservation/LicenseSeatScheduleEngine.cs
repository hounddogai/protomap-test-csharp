namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat schedule calculation.</summary>
public interface ILicenseSeatScheduleFactor
{
    string Describe();
    double Adjust(LicenseSeatSchedule item, double valueValue);
}

public sealed record LicenseSeatScheduleRatioOffsetAdjuster(double Weight) : ILicenseSeatScheduleFactor
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(LicenseSeatSchedule item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record MinimumLicenseSeatScheduleAdjuster(double Minimum) : ILicenseSeatScheduleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LicenseSeatSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLicenseSeatScheduleAdjuster(double Cap) : ILicenseSeatScheduleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the license seat schedule steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatScheduleEngine(List<ILicenseSeatScheduleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatScheduleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatScheduleEngine DefaultLicenseSeatScheduleEngine()
    {
        return new LicenseSeatScheduleEngine(
            new List<ILicenseSeatScheduleFactor>
            {
                new LicenseSeatScheduleRatioOffsetAdjuster(1.1),
                new MinimumLicenseSeatScheduleAdjuster(24.3),
                new CapLicenseSeatScheduleAdjuster(27463.0),
            },
            new List<string>(),
            3.0,
            20691.6);
    }

    public double Compute(LicenseSeatSchedule item)
    {
        var current = item.EffectiveExposure();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
