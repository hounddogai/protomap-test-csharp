namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adjustment ledger calculation.</summary>
public interface IAdjustmentLedgerFactor
{
    string Describe();
    double Adjust(AdjustmentLedger item, double valueValue);
}

public sealed record FlaggedAdjustmentLedgerAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentLedgerFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdjustmentLedgerAdjuster(double Minimum) : IAdjustmentLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdjustmentLedgerAdjuster(double Cap) : IAdjustmentLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionAdjustmentLedgerAdjuster(Dictionary<string, double> Factors) : IAdjustmentLedgerFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(AdjustmentLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adjustment ledger steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentLedgerEngine(List<IAdjustmentLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentLedgerEngine DefaultAdjustmentLedgerEngine(Registry flags)
    {
        return new AdjustmentLedgerEngine(
            new List<IAdjustmentLedgerFactor>
            {
                new FlaggedAdjustmentLedgerAdjuster(flags, "adjustment-ledger-fallback", 0.2),
                new MinimumAdjustmentLedgerAdjuster(3.4),
                new CapAdjustmentLedgerAdjuster(48880.0),
                new RegionAdjustmentLedgerAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.34, ["central"] = 1.3 }),
            },
            new List<string>(),
            2.8,
            17687.7);
    }

    public double Price(AdjustmentLedger item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
