namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return policy calculation.</summary>
public interface IAssetReturnPolicyAdjuster
{
    string Describe();
    double Adjust(AssetReturnPolicy item, double valueValue);
}

public sealed record FlaggedAssetReturnPolicyAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAssetReturnPolicyAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnPolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnPolicy item, double valueValue)
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

public sealed record PercentAssetReturnPolicyAdjuster(double Percent) : IAssetReturnPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssetReturnPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAssetReturnPolicyAdjuster(double Cap) : IAssetReturnPolicyAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssetReturnPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAssetReturnPolicyAdjuster(double Minimum) : IAssetReturnPolicyAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAssetReturnPolicyAdjuster(double Step) : IAssetReturnPolicyAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssetReturnPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the asset return policy steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnPolicyEngine(List<IAssetReturnPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnPolicyEngine DefaultAssetReturnPolicyEngine(Registry flags)
    {
        return new AssetReturnPolicyEngine(
            new List<IAssetReturnPolicyAdjuster>
            {
                new FlaggedAssetReturnPolicyAdjuster(flags, "asset-return-policy-capped", -8.5),
                new TieredAssetReturnPolicyAdjuster(
                    new List<double> { 1433.0, 6348.0, 8743.0, 8842.0 },
                    new List<double> { 0.91, 1.19, 0.9, 0.82 }),
                new PercentAssetReturnPolicyAdjuster(-3.6),
                new CapAssetReturnPolicyAdjuster(32619.0),
                new MinimumAssetReturnPolicyAdjuster(31.9),
                new RoundingAssetReturnPolicyAdjuster(0.05),
            },
            new List<string>(),
            1.2,
            33500.6);
    }

    public double Compute(AssetReturnPolicy item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
