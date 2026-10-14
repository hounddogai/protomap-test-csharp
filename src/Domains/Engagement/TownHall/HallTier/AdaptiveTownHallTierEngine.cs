namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive town hall tier calculation.</summary>
public interface IAdaptiveTownHallTierModifier
{
    string Describe();
    double Adjust(AdaptiveTownHallTier item, double valueValue);
}

public sealed record AdaptiveTownHallTierBudgetOffsetAdjuster(double Weight) : IAdaptiveTownHallTierModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(AdaptiveTownHallTier item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record CapAdaptiveTownHallTierAdjuster(double Cap) : IAdaptiveTownHallTierModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveTownHallTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAdaptiveTownHallTierAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveTownHallTierModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveTownHallTier item, double valueValue)
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

public sealed record FlaggedAdaptiveTownHallTierAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveTownHallTierModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveTownHallTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive town hall tier steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTownHallTierEngine(List<IAdaptiveTownHallTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTownHallTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTownHallTierEngine DefaultAdaptiveTownHallTierEngine(Registry flags)
    {
        return new AdaptiveTownHallTierEngine(
            new List<IAdaptiveTownHallTierModifier>
            {
                new AdaptiveTownHallTierBudgetOffsetAdjuster(0.65),
                new CapAdaptiveTownHallTierAdjuster(5316.0),
                new TieredAdaptiveTownHallTierAdjuster(
                    new List<double> { 1844.0, 7471.0, 7725.0 },
                    new List<double> { 1.2, 1.19, 0.9 }),
                new FlaggedAdaptiveTownHallTierAdjuster(flags, "adaptive-town-hall-tier-legacy", -5.2),
            },
            new List<string>(),
            2.4,
            51847.6);
    }

    public double Compute(AdaptiveTownHallTier item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTownHallTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
