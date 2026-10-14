namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped access badge threshold calculation.</summary>
public interface ICappedAccessBadgeThresholdStep
{
    string Describe();
    double Adjust(CappedAccessBadgeThreshold item, double valueValue);
}

public sealed record PercentCappedAccessBadgeThresholdAdjuster(double Percent) : ICappedAccessBadgeThresholdStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedAccessBadgeThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedAccessBadgeThresholdAdjuster(double Step) : ICappedAccessBadgeThresholdStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedAccessBadgeThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCappedAccessBadgeThresholdAdjuster(double Minimum) : ICappedAccessBadgeThresholdStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedAccessBadgeThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCappedAccessBadgeThresholdAdjuster(List<double> Bounds, List<double> Rates) : ICappedAccessBadgeThresholdStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedAccessBadgeThreshold item, double valueValue)
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

public sealed record FlaggedCappedAccessBadgeThresholdAdjuster(Registry Flags, string Flag, double Percent) : ICappedAccessBadgeThresholdStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedAccessBadgeThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped access badge threshold steps in order and keeps a trace of each.</summary>
public sealed class CappedAccessBadgeThresholdEngine(List<ICappedAccessBadgeThresholdStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAccessBadgeThresholdStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAccessBadgeThresholdEngine DefaultCappedAccessBadgeThresholdEngine(Registry flags)
    {
        return new CappedAccessBadgeThresholdEngine(
            new List<ICappedAccessBadgeThresholdStep>
            {
                new PercentCappedAccessBadgeThresholdAdjuster(-7.8),
                new RoundingCappedAccessBadgeThresholdAdjuster(0.1),
                new MinimumCappedAccessBadgeThresholdAdjuster(3.7),
                new TieredCappedAccessBadgeThresholdAdjuster(
                    new List<double> { 3384.0, 8066.0 },
                    new List<double> { 1.08, 1.07 }),
                new FlaggedCappedAccessBadgeThresholdAdjuster(flags, "capped-access-badge-threshold-scheduled", 3.3),
            },
            new List<string>(),
            4.8,
            1320.2);
    }

    public double Compute(CappedAccessBadgeThreshold item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedAccessBadgeThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
