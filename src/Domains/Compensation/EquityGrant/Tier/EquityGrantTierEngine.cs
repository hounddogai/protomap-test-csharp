namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant tier calculation.</summary>
public interface IEquityGrantTierModifier
{
    string Describe();
    double Adjust(EquityGrantTier item, double valueValue);
}

public sealed record FlaggedEquityGrantTierAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantTierModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumEquityGrantTierAdjuster(double Minimum) : IEquityGrantTierModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredEquityGrantTierAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantTierModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantTier item, double valueValue)
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

public sealed record ChannelEquityGrantTierAdjuster(Dictionary<string, double> Factors) : IEquityGrantTierModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(EquityGrantTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the equity grant tier steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantTierEngine(List<IEquityGrantTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantTierEngine DefaultEquityGrantTierEngine(Registry flags)
    {
        return new EquityGrantTierEngine(
            new List<IEquityGrantTierModifier>
            {
                new FlaggedEquityGrantTierAdjuster(flags, "equity-grant-tier-primary", -0.4),
                new MinimumEquityGrantTierAdjuster(7.9),
                new TieredEquityGrantTierAdjuster(
                    new List<double> { 7383.0, 7403.0 },
                    new List<double> { 1.17, 1.11 }),
                new ChannelEquityGrantTierAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.92, ["retail"] = 0.77 }),
            },
            new List<string>(),
            3.6,
            63685.5);
    }

    public double Run(EquityGrantTier item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
