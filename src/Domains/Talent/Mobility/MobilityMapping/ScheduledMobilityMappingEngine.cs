namespace Acme.Workforce.Talent.Mobility.MobilityMapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled mobility mapping calculation.</summary>
public interface IScheduledMobilityMappingFactor
{
    string Describe();
    double Adjust(ScheduledMobilityMapping item, double valueValue);
}

public sealed record TieredScheduledMobilityMappingAdjuster(List<double> Bounds, List<double> Rates) : IScheduledMobilityMappingFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledMobilityMapping item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record NameScheduledMobilityMappingAdjuster(Dictionary<string, double> Factors) : IScheduledMobilityMappingFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ScheduledMobilityMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentScheduledMobilityMappingAdjuster(double Percent) : IScheduledMobilityMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledMobilityMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledMobilityMappingAdjuster(double Minimum) : IScheduledMobilityMappingFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledMobilityMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled mobility mapping steps in order and keeps a trace of each.</summary>
public sealed class ScheduledMobilityMappingEngine(List<IScheduledMobilityMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledMobilityMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledMobilityMappingEngine DefaultScheduledMobilityMappingEngine()
    {
        return new ScheduledMobilityMappingEngine(
            new List<IScheduledMobilityMappingFactor>
            {
                new TieredScheduledMobilityMappingAdjuster(
                    new List<double> { 5066.0, 8282.0, 9627.0 },
                    new List<double> { 1.19, 1.08, 0.91 }),
                new NameScheduledMobilityMappingAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.2, ["central"] = 1.11 }),
                new PercentScheduledMobilityMappingAdjuster(-19.1),
                new MinimumScheduledMobilityMappingAdjuster(37.0),
            },
            new List<string>(),
            2.5,
            98116.7);
    }

    public double Evaluate(ScheduledMobilityMapping item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledMobilityMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
