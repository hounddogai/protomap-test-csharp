namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered equity grant ledger calculation.</summary>
public interface ITieredEquityGrantLedgerStep
{
    string Describe();
    double Adjust(TieredEquityGrantLedger item, double valueValue);
}

public sealed record RegionTieredEquityGrantLedgerAdjuster(Dictionary<string, double> Factors) : ITieredEquityGrantLedgerStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(TieredEquityGrantLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTieredEquityGrantLedgerAdjuster(List<double> Bounds, List<double> Rates) : ITieredEquityGrantLedgerStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredEquityGrantLedger item, double valueValue)
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

public sealed record FlaggedTieredEquityGrantLedgerAdjuster(Registry Flags, string Flag, double Percent) : ITieredEquityGrantLedgerStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredEquityGrantLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered equity grant ledger steps in order and keeps a trace of each.</summary>
public sealed class TieredEquityGrantLedgerEngine(List<ITieredEquityGrantLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredEquityGrantLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredEquityGrantLedgerEngine DefaultTieredEquityGrantLedgerEngine(Registry flags)
    {
        return new TieredEquityGrantLedgerEngine(
            new List<ITieredEquityGrantLedgerStep>
            {
                new RegionTieredEquityGrantLedgerAdjuster(new Dictionary<string, double> { ["south"] = 1.38 }),
                new TieredTieredEquityGrantLedgerAdjuster(
                    new List<double> { 3089.0, 5536.0, 6107.0 },
                    new List<double> { 1.15, 0.92, 1.11 }),
                new FlaggedTieredEquityGrantLedgerAdjuster(flags, "tiered-equity-grant-ledger-adaptive", 7.1),
            },
            new List<string>(),
            4.5,
            41346.0);
    }

    public double Compute(TieredEquityGrantLedger item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredEquityGrantLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
