namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default access badge bundle calculation.</summary>
public interface IDefaultAccessBadgeBundleModifier
{
    string Describe();
    double Adjust(DefaultAccessBadgeBundle item, double valueValue);
}

public sealed record TieredDefaultAccessBadgeBundleAdjuster(List<double> Bounds, List<double> Rates) : IDefaultAccessBadgeBundleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultAccessBadgeBundle item, double valueValue)
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

public sealed record FlaggedDefaultAccessBadgeBundleAdjuster(Registry Flags, string Flag, double Percent) : IDefaultAccessBadgeBundleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultAccessBadgeBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultAccessBadgeBundleAdjuster(double Minimum) : IDefaultAccessBadgeBundleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultAccessBadgeBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultAccessBadgeBundleAdjuster(double Percent) : IDefaultAccessBadgeBundleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultAccessBadgeBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultAccessBadgeBundleAdjuster(double Step) : IDefaultAccessBadgeBundleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultAccessBadgeBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapDefaultAccessBadgeBundleAdjuster(double Cap) : IDefaultAccessBadgeBundleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultAccessBadgeBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default access badge bundle steps in order and keeps a trace of each.</summary>
public sealed class DefaultAccessBadgeBundleEngine(List<IDefaultAccessBadgeBundleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultAccessBadgeBundleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultAccessBadgeBundleEngine DefaultDefaultAccessBadgeBundleEngine(Registry flags)
    {
        return new DefaultAccessBadgeBundleEngine(
            new List<IDefaultAccessBadgeBundleModifier>
            {
                new TieredDefaultAccessBadgeBundleAdjuster(
                    new List<double> { 1778.0, 1939.0, 7592.0, 8558.0 },
                    new List<double> { 0.96, 0.95, 1.13, 0.99 }),
                new FlaggedDefaultAccessBadgeBundleAdjuster(flags, "default-access-badge-bundle-pending", -5.0),
                new MinimumDefaultAccessBadgeBundleAdjuster(39.6),
                new PercentDefaultAccessBadgeBundleAdjuster(-8.2),
                new RoundingDefaultAccessBadgeBundleAdjuster(0.05),
                new CapDefaultAccessBadgeBundleAdjuster(2478.0),
            },
            new List<string>(),
            1.8,
            2757.6);
    }

    public double Run(DefaultAccessBadgeBundle item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultAccessBadgeBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
