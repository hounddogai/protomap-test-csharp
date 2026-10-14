namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>One step of the regional access badge threshold calculation.</summary>
public interface IRegionalAccessBadgeThresholdFactor
{
    string Describe();
    double Adjust(RegionalAccessBadgeThreshold item, double valueValue);
}

public sealed record CapRegionalAccessBadgeThresholdAdjuster(double Cap) : IRegionalAccessBadgeThresholdFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalAccessBadgeThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRegionalAccessBadgeThresholdAdjuster(double Percent) : IRegionalAccessBadgeThresholdFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalAccessBadgeThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRegionalAccessBadgeThresholdAdjuster(List<double> Bounds, List<double> Rates) : IRegionalAccessBadgeThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalAccessBadgeThreshold item, double valueValue)
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

/// <summary>Runs the regional access badge threshold steps in order and keeps a trace of each.</summary>
public sealed class RegionalAccessBadgeThresholdEngine(List<IRegionalAccessBadgeThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalAccessBadgeThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalAccessBadgeThresholdEngine DefaultRegionalAccessBadgeThresholdEngine()
    {
        return new RegionalAccessBadgeThresholdEngine(
            new List<IRegionalAccessBadgeThresholdFactor>
            {
                new CapRegionalAccessBadgeThresholdAdjuster(32501.0),
                new PercentRegionalAccessBadgeThresholdAdjuster(-16.1),
                new TieredRegionalAccessBadgeThresholdAdjuster(
                    new List<double> { 844.0, 9207.0 },
                    new List<double> { 1.08, 1.15 }),
            },
            new List<string>(),
            3.3,
            73491.2);
    }

    public double Run(RegionalAccessBadgeThreshold item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalAccessBadgeThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
