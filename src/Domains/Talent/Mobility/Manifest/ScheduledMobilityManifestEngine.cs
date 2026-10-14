namespace Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>One step of the scheduled mobility manifest calculation.</summary>
public interface IScheduledMobilityManifestAdjuster
{
    string Describe();
    double Adjust(ScheduledMobilityManifest item, double valueValue);
}

public sealed record ScheduledMobilityManifestMinimumOffsetAdjuster(double Weight) : IScheduledMobilityManifestAdjuster
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(ScheduledMobilityManifest item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record TieredScheduledMobilityManifestAdjuster(List<double> Bounds, List<double> Rates) : IScheduledMobilityManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledMobilityManifest item, double valueValue)
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

public sealed record LocaleScheduledMobilityManifestAdjuster(Dictionary<string, double> Factors) : IScheduledMobilityManifestAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(ScheduledMobilityManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scheduled mobility manifest steps in order and keeps a trace of each.</summary>
public sealed class ScheduledMobilityManifestEngine(List<IScheduledMobilityManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledMobilityManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledMobilityManifestEngine DefaultScheduledMobilityManifestEngine()
    {
        return new ScheduledMobilityManifestEngine(
            new List<IScheduledMobilityManifestAdjuster>
            {
                new ScheduledMobilityManifestMinimumOffsetAdjuster(2.0),
                new TieredScheduledMobilityManifestAdjuster(
                    new List<double> { 1176.0, 1542.0, 4343.0 },
                    new List<double> { 0.91, 1.19, 0.93 }),
                new LocaleScheduledMobilityManifestAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.95, ["south"] = 1.04 }),
            },
            new List<string>(),
            4.3,
            13539.4);
    }

    public double Price(ScheduledMobilityManifest item)
    {
        var current = item.AdjustedLoad();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<ScheduledMobilityManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
