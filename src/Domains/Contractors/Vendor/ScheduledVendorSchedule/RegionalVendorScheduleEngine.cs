namespace Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional vendor schedule calculation.</summary>
public interface IRegionalVendorScheduleStep
{
    string Describe();
    double Adjust(RegionalVendorSchedule item, double valueValue);
}

public sealed record RegionalVendorScheduleThresholdOffsetAdjuster(double Weight) : IRegionalVendorScheduleStep
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(RegionalVendorSchedule item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record RoundingRegionalVendorScheduleAdjuster(double Step) : IRegionalVendorScheduleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalVendorSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRegionalVendorScheduleAdjuster(double Cap) : IRegionalVendorScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalVendorSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRegionalVendorScheduleAdjuster(double Percent) : IRegionalVendorScheduleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalVendorSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional vendor schedule steps in order and keeps a trace of each.</summary>
public sealed class RegionalVendorScheduleEngine(List<IRegionalVendorScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalVendorScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalVendorScheduleEngine DefaultRegionalVendorScheduleEngine()
    {
        return new RegionalVendorScheduleEngine(
            new List<IRegionalVendorScheduleStep>
            {
                new RegionalVendorScheduleThresholdOffsetAdjuster(1.74),
                new RoundingRegionalVendorScheduleAdjuster(0.5),
                new CapRegionalVendorScheduleAdjuster(27583.0),
                new PercentRegionalVendorScheduleAdjuster(-23.1),
            },
            new List<string>(),
            3.3,
            67568.3);
    }

    public double Price(RegionalVendorSchedule item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalVendorSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
