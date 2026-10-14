namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge threshold calculation.</summary>
public interface IAccessBadgeThresholdAdjuster
{
    string Describe();
    double Adjust(AccessBadgeThreshold item, double valueValue);
}

public sealed record RoundingAccessBadgeThresholdAdjuster(double Step) : IAccessBadgeThresholdAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAccessBadgeThresholdAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeThreshold item, double valueValue)
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

public sealed record PercentAccessBadgeThresholdAdjuster(double Percent) : IAccessBadgeThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the access badge threshold steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeThresholdEngine(List<IAccessBadgeThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeThresholdEngine DefaultAccessBadgeThresholdEngine()
    {
        return new AccessBadgeThresholdEngine(
            new List<IAccessBadgeThresholdAdjuster>
            {
                new RoundingAccessBadgeThresholdAdjuster(0.25),
                new TieredAccessBadgeThresholdAdjuster(
                    new List<double> { 2736.0, 6475.0, 7438.0, 9787.0 },
                    new List<double> { 1.09, 1.1, 0.97, 1.02 }),
                new PercentAccessBadgeThresholdAdjuster(-9.0),
            },
            new List<string>(),
            0.8,
            4400.1);
    }

    public double Evaluate(AccessBadgeThreshold item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
