namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default access badge rollup calculation.</summary>
public interface IDefaultAccessBadgeRollupAdjuster
{
    string Describe();
    double Adjust(DefaultAccessBadgeRollup item, double valueValue);
}

public sealed record CapDefaultAccessBadgeRollupAdjuster(double Cap) : IDefaultAccessBadgeRollupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultAccessBadgeRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultAccessBadgeRollupAdjuster(Registry Flags, string Flag, double Percent) : IDefaultAccessBadgeRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultAccessBadgeRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultAccessBadgeRollupAdjuster(List<double> Bounds, List<double> Rates) : IDefaultAccessBadgeRollupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultAccessBadgeRollup item, double valueValue)
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

public sealed record MinimumDefaultAccessBadgeRollupAdjuster(double Minimum) : IDefaultAccessBadgeRollupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultAccessBadgeRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingDefaultAccessBadgeRollupAdjuster(double Step) : IDefaultAccessBadgeRollupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultAccessBadgeRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultAccessBadgeRollupAdjuster(double Percent) : IDefaultAccessBadgeRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultAccessBadgeRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default access badge rollup steps in order and keeps a trace of each.</summary>
public sealed class DefaultAccessBadgeRollupEngine(List<IDefaultAccessBadgeRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultAccessBadgeRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultAccessBadgeRollupEngine DefaultDefaultAccessBadgeRollupEngine(Registry flags)
    {
        return new DefaultAccessBadgeRollupEngine(
            new List<IDefaultAccessBadgeRollupAdjuster>
            {
                new CapDefaultAccessBadgeRollupAdjuster(31844.0),
                new FlaggedDefaultAccessBadgeRollupAdjuster(flags, "default-access-badge-rollup-seasonal", 1.8),
                new TieredDefaultAccessBadgeRollupAdjuster(
                    new List<double> { 1444.0, 5506.0, 7960.0 },
                    new List<double> { 1.18, 0.85, 1.07 }),
                new MinimumDefaultAccessBadgeRollupAdjuster(11.2),
                new RoundingDefaultAccessBadgeRollupAdjuster(0.5),
                new PercentDefaultAccessBadgeRollupAdjuster(-0.5),
            },
            new List<string>(),
            1.6,
            74999.0);
    }

    public double Compute(DefaultAccessBadgeRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultAccessBadgeRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
