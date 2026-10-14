namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered asset return ledger calculation.</summary>
public interface ITieredAssetReturnLedgerModifier
{
    string Describe();
    double Adjust(TieredAssetReturnLedger item, double valueValue);
}

public sealed record SegmentTieredAssetReturnLedgerAdjuster(Dictionary<string, double> Factors) : ITieredAssetReturnLedgerModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TieredAssetReturnLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTieredAssetReturnLedgerAdjuster(List<double> Bounds, List<double> Rates) : ITieredAssetReturnLedgerModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredAssetReturnLedger item, double valueValue)
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

public sealed record FlaggedTieredAssetReturnLedgerAdjuster(Registry Flags, string Flag, double Percent) : ITieredAssetReturnLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredAssetReturnLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredAssetReturnLedgerAdjuster(double Step) : ITieredAssetReturnLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredAssetReturnLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tiered asset return ledger steps in order and keeps a trace of each.</summary>
public sealed class TieredAssetReturnLedgerEngine(List<ITieredAssetReturnLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredAssetReturnLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredAssetReturnLedgerEngine DefaultTieredAssetReturnLedgerEngine(Registry flags)
    {
        return new TieredAssetReturnLedgerEngine(
            new List<ITieredAssetReturnLedgerModifier>
            {
                new SegmentTieredAssetReturnLedgerAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.89, ["east"] = 0.81, ["retail"] = 0.77 }),
                new TieredTieredAssetReturnLedgerAdjuster(
                    new List<double> { 285.0, 1592.0, 3904.0, 7947.0 },
                    new List<double> { 0.97, 0.95, 1.04, 1.17 }),
                new FlaggedTieredAssetReturnLedgerAdjuster(flags, "tiered-asset-return-ledger-default", -2.9),
                new RoundingTieredAssetReturnLedgerAdjuster(1.0),
            },
            new List<string>(),
            3.0,
            43316.7);
    }

    public double Compute(TieredAssetReturnLedger item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredAssetReturnLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
