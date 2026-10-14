namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default access badge override calculation.</summary>
public interface IDefaultAccessBadgeOverrideModifier
{
    string Describe();
    double Adjust(DefaultAccessBadgeOverride item, double valueValue);
}

public sealed record RoundingDefaultAccessBadgeOverrideAdjuster(double Step) : IDefaultAccessBadgeOverrideModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultAccessBadgeOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DefaultAccessBadgeOverrideMinimumOffsetAdjuster(double Weight) : IDefaultAccessBadgeOverrideModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(DefaultAccessBadgeOverride item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record FlaggedDefaultAccessBadgeOverrideAdjuster(Registry Flags, string Flag, double Percent) : IDefaultAccessBadgeOverrideModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultAccessBadgeOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultAccessBadgeOverrideAdjuster(List<double> Bounds, List<double> Rates) : IDefaultAccessBadgeOverrideModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultAccessBadgeOverride item, double valueValue)
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

/// <summary>Runs the default access badge override steps in order and keeps a trace of each.</summary>
public sealed class DefaultAccessBadgeOverrideEngine(List<IDefaultAccessBadgeOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultAccessBadgeOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultAccessBadgeOverrideEngine DefaultDefaultAccessBadgeOverrideEngine(Registry flags)
    {
        return new DefaultAccessBadgeOverrideEngine(
            new List<IDefaultAccessBadgeOverrideModifier>
            {
                new RoundingDefaultAccessBadgeOverrideAdjuster(5.0),
                new DefaultAccessBadgeOverrideMinimumOffsetAdjuster(1.52),
                new FlaggedDefaultAccessBadgeOverrideAdjuster(flags, "default-access-badge-override-primary", -4.5),
                new TieredDefaultAccessBadgeOverrideAdjuster(
                    new List<double> { 4889.0, 9532.0, 9970.0 },
                    new List<double> { 0.83, 0.88, 1.09 }),
            },
            new List<string>(),
            1.0,
            15281.2);
    }

    public double Evaluate(DefaultAccessBadgeOverride item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultAccessBadgeOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
