namespace Acme.Workforce.Assets.AssetReturn.Tier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling asset return baseline calculation.</summary>
public interface IRollingAssetReturnBaselineStep
{
    string Describe();
    double Adjust(RollingAssetReturnBaseline item, double valueValue);
}

public sealed record MinimumRollingAssetReturnBaselineAdjuster(double Minimum) : IRollingAssetReturnBaselineStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingAssetReturnBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelRollingAssetReturnBaselineAdjuster(Dictionary<string, double> Factors) : IRollingAssetReturnBaselineStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RollingAssetReturnBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRollingAssetReturnBaselineAdjuster(List<double> Bounds, List<double> Rates) : IRollingAssetReturnBaselineStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingAssetReturnBaseline item, double valueValue)
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

public sealed record CapRollingAssetReturnBaselineAdjuster(double Cap) : IRollingAssetReturnBaselineStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingAssetReturnBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRollingAssetReturnBaselineAdjuster(Registry Flags, string Flag, double Percent) : IRollingAssetReturnBaselineStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingAssetReturnBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRollingAssetReturnBaselineAdjuster(double Percent) : IRollingAssetReturnBaselineStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingAssetReturnBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling asset return baseline steps in order and keeps a trace of each.</summary>
public sealed class RollingAssetReturnBaselineEngine(List<IRollingAssetReturnBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingAssetReturnBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingAssetReturnBaselineEngine DefaultRollingAssetReturnBaselineEngine(Registry flags)
    {
        return new RollingAssetReturnBaselineEngine(
            new List<IRollingAssetReturnBaselineStep>
            {
                new MinimumRollingAssetReturnBaselineAdjuster(38.8),
                new LabelRollingAssetReturnBaselineAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.99, ["online"] = 1.11 }),
                new TieredRollingAssetReturnBaselineAdjuster(
                    new List<double> { 182.0, 6425.0 },
                    new List<double> { 0.98, 0.8 }),
                new CapRollingAssetReturnBaselineAdjuster(17978.0),
                new FlaggedRollingAssetReturnBaselineAdjuster(flags, "rolling-asset-return-baseline-rolling", -6.5),
                new PercentRollingAssetReturnBaselineAdjuster(11.1),
            },
            new List<string>(),
            1.1,
            44853.0);
    }

    public double Price(RollingAssetReturnBaseline item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingAssetReturnBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
