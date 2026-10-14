namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge variance calculation.</summary>
public interface IAccessBadgeVarianceModifier
{
    string Describe();
    double Adjust(AccessBadgeVariance item, double valueValue);
}

public sealed record MinimumAccessBadgeVarianceAdjuster(double Minimum) : IAccessBadgeVarianceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAccessBadgeVarianceAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeVarianceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeVariance item, double valueValue)
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

public sealed record PercentAccessBadgeVarianceAdjuster(double Percent) : IAccessBadgeVarianceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAccessBadgeVarianceAdjuster(double Step) : IAccessBadgeVarianceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAccessBadgeVarianceAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeVarianceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitAccessBadgeVarianceAdjuster(Dictionary<string, double> Factors) : IAccessBadgeVarianceModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AccessBadgeVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the access badge variance steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeVarianceEngine(List<IAccessBadgeVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeVarianceEngine DefaultAccessBadgeVarianceEngine(Registry flags)
    {
        return new AccessBadgeVarianceEngine(
            new List<IAccessBadgeVarianceModifier>
            {
                new MinimumAccessBadgeVarianceAdjuster(7.7),
                new TieredAccessBadgeVarianceAdjuster(
                    new List<double> { 339.0, 3911.0, 5625.0, 7740.0 },
                    new List<double> { 0.86, 1.05, 1.0, 0.92 }),
                new PercentAccessBadgeVarianceAdjuster(-5.8),
                new RoundingAccessBadgeVarianceAdjuster(1.0),
                new FlaggedAccessBadgeVarianceAdjuster(flags, "access-badge-variance-seasonal", 0.1),
                new UnitAccessBadgeVarianceAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.81, ["west"] = 1.21 }),
            },
            new List<string>(),
            3.5,
            49946.8);
    }

    public double Price(AccessBadgeVariance item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
