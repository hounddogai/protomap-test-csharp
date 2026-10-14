namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback asset return draft calculation.</summary>
public interface IFallbackAssetReturnDraftModifier
{
    string Describe();
    double Adjust(FallbackAssetReturnDraft item, double valueValue);
}

public sealed record RoundingFallbackAssetReturnDraftAdjuster(double Step) : IFallbackAssetReturnDraftModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackAssetReturnDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFallbackAssetReturnDraftAdjuster(double Minimum) : IFallbackAssetReturnDraftModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackAssetReturnDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedFallbackAssetReturnDraftAdjuster(Registry Flags, string Flag, double Percent) : IFallbackAssetReturnDraftModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackAssetReturnDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback asset return draft steps in order and keeps a trace of each.</summary>
public sealed class FallbackAssetReturnDraftEngine(List<IFallbackAssetReturnDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackAssetReturnDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackAssetReturnDraftEngine DefaultFallbackAssetReturnDraftEngine(Registry flags)
    {
        return new FallbackAssetReturnDraftEngine(
            new List<IFallbackAssetReturnDraftModifier>
            {
                new RoundingFallbackAssetReturnDraftAdjuster(0.25),
                new MinimumFallbackAssetReturnDraftAdjuster(24.4),
                new FlaggedFallbackAssetReturnDraftAdjuster(flags, "fallback-asset-return-draft-weighted", -2.3),
            },
            new List<string>(),
            4.4,
            74759.5);
    }

    public double Price(FallbackAssetReturnDraft item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackAssetReturnDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
