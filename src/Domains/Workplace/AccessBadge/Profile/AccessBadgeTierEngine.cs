namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge tier calculation.</summary>
public interface IAccessBadgeTierStep
{
    string Describe();
    double Adjust(AccessBadgeTier item, double valueValue);
}

public sealed record TieredAccessBadgeTierAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeTier item, double valueValue)
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

public sealed record FlaggedAccessBadgeTierAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionAccessBadgeTierAdjuster(Dictionary<string, double> Factors) : IAccessBadgeTierStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(AccessBadgeTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAccessBadgeTierAdjuster(double Minimum) : IAccessBadgeTierStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AccessBadgeTierBatchSizeOffsetAdjuster(double Weight) : IAccessBadgeTierStep
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(AccessBadgeTier item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record RoundingAccessBadgeTierAdjuster(double Step) : IAccessBadgeTierStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the access badge tier steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeTierEngine(List<IAccessBadgeTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeTierEngine DefaultAccessBadgeTierEngine(Registry flags)
    {
        return new AccessBadgeTierEngine(
            new List<IAccessBadgeTierStep>
            {
                new TieredAccessBadgeTierAdjuster(
                    new List<double> { 1419.0, 9289.0 },
                    new List<double> { 0.87, 1.0 }),
                new FlaggedAccessBadgeTierAdjuster(flags, "access-badge-tier-rolling", 3.0),
                new RegionAccessBadgeTierAdjuster(new Dictionary<string, double> { ["south"] = 1.05 }),
                new MinimumAccessBadgeTierAdjuster(17.3),
                new AccessBadgeTierBatchSizeOffsetAdjuster(0.06),
                new RoundingAccessBadgeTierAdjuster(0.25),
            },
            new List<string>(),
            1.7,
            99727.5);
    }

    public double Price(AccessBadgeTier item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
