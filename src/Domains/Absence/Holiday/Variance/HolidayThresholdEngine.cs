namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday threshold calculation.</summary>
public interface IHolidayThresholdStep
{
    string Describe();
    double Adjust(HolidayThreshold item, double valueValue);
}

public sealed record FlaggedHolidayThresholdAdjuster(Registry Flags, string Flag, double Percent) : IHolidayThresholdStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HolidayThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapHolidayThresholdAdjuster(double Cap) : IHolidayThresholdStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidayThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumHolidayThresholdAdjuster(double Minimum) : IHolidayThresholdStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the holiday threshold steps in order and keeps a trace of each.</summary>
public sealed class HolidayThresholdEngine(List<IHolidayThresholdStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayThresholdStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayThresholdEngine DefaultHolidayThresholdEngine(Registry flags)
    {
        return new HolidayThresholdEngine(
            new List<IHolidayThresholdStep>
            {
                new FlaggedHolidayThresholdAdjuster(flags, "holiday-threshold-primary", 7.9),
                new CapHolidayThresholdAdjuster(19374.0),
                new MinimumHolidayThresholdAdjuster(39.1),
            },
            new List<string>(),
            1.7,
            20831.9);
    }

    public double Compute(HolidayThreshold item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
