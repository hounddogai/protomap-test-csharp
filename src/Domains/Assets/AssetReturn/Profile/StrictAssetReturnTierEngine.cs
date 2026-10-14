namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict asset return tier calculation.</summary>
public interface IStrictAssetReturnTierFactor
{
    string Describe();
    double Adjust(StrictAssetReturnTier item, double valueValue);
}

public sealed record TieredStrictAssetReturnTierAdjuster(List<double> Bounds, List<double> Rates) : IStrictAssetReturnTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictAssetReturnTier item, double valueValue)
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

public sealed record FlaggedStrictAssetReturnTierAdjuster(Registry Flags, string Flag, double Percent) : IStrictAssetReturnTierFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictAssetReturnTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStrictAssetReturnTierAdjuster(double Step) : IStrictAssetReturnTierFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictAssetReturnTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStrictAssetReturnTierAdjuster(double Minimum) : IStrictAssetReturnTierFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictAssetReturnTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record StrictAssetReturnTierLeaveDaysOffsetAdjuster(double Weight) : IStrictAssetReturnTierFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(StrictAssetReturnTier item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record CapStrictAssetReturnTierAdjuster(double Cap) : IStrictAssetReturnTierFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictAssetReturnTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the strict asset return tier steps in order and keeps a trace of each.</summary>
public sealed class StrictAssetReturnTierEngine(List<IStrictAssetReturnTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAssetReturnTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAssetReturnTierEngine DefaultStrictAssetReturnTierEngine(Registry flags)
    {
        return new StrictAssetReturnTierEngine(
            new List<IStrictAssetReturnTierFactor>
            {
                new TieredStrictAssetReturnTierAdjuster(
                    new List<double> { 965.0, 7853.0, 7926.0 },
                    new List<double> { 1.05, 1.0, 1.05 }),
                new FlaggedStrictAssetReturnTierAdjuster(flags, "strict-asset-return-tier-weighted", -7.4),
                new RoundingStrictAssetReturnTierAdjuster(0.05),
                new MinimumStrictAssetReturnTierAdjuster(32.8),
                new StrictAssetReturnTierLeaveDaysOffsetAdjuster(1.7),
                new CapStrictAssetReturnTierAdjuster(44716.0),
            },
            new List<string>(),
            0.1,
            60919.9);
    }

    public double Compute(StrictAssetReturnTier item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StrictAssetReturnTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
