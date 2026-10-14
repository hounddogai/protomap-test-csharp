namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback asset return preference calculation.</summary>
public interface IFallbackAssetReturnPreferenceAdjuster
{
    string Describe();
    double Adjust(FallbackAssetReturnPreference item, double valueValue);
}

public sealed record FlaggedFallbackAssetReturnPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IFallbackAssetReturnPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackAssetReturnPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackAssetReturnPreferenceAdjuster(double Cap) : IFallbackAssetReturnPreferenceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackAssetReturnPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FallbackAssetReturnPreferenceCapacityOffsetAdjuster(double Weight) : IFallbackAssetReturnPreferenceAdjuster
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(FallbackAssetReturnPreference item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record TieredFallbackAssetReturnPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IFallbackAssetReturnPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackAssetReturnPreference item, double valueValue)
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

public sealed record RoundingFallbackAssetReturnPreferenceAdjuster(double Step) : IFallbackAssetReturnPreferenceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackAssetReturnPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFallbackAssetReturnPreferenceAdjuster(double Percent) : IFallbackAssetReturnPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackAssetReturnPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback asset return preference steps in order and keeps a trace of each.</summary>
public sealed class FallbackAssetReturnPreferenceEngine(List<IFallbackAssetReturnPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackAssetReturnPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackAssetReturnPreferenceEngine DefaultFallbackAssetReturnPreferenceEngine(Registry flags)
    {
        return new FallbackAssetReturnPreferenceEngine(
            new List<IFallbackAssetReturnPreferenceAdjuster>
            {
                new FlaggedFallbackAssetReturnPreferenceAdjuster(
                    flags,
                    "fallback-asset-return-preference-seasonal",
                    -9.8),
                new CapFallbackAssetReturnPreferenceAdjuster(17119.0),
                new FallbackAssetReturnPreferenceCapacityOffsetAdjuster(1.09),
                new TieredFallbackAssetReturnPreferenceAdjuster(
                    new List<double> { 5844.0, 6448.0, 8524.0 },
                    new List<double> { 0.83, 1.09, 0.98 }),
                new RoundingFallbackAssetReturnPreferenceAdjuster(1.0),
                new PercentFallbackAssetReturnPreferenceAdjuster(10.0),
            },
            new List<string>(),
            3.0,
            96023.6);
    }

    public double Run(FallbackAssetReturnPreference item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackAssetReturnPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
