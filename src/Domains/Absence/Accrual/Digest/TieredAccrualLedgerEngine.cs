namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered accrual ledger calculation.</summary>
public interface ITieredAccrualLedgerFactor
{
    string Describe();
    double Adjust(TieredAccrualLedger item, double valueValue);
}

public sealed record LocaleTieredAccrualLedgerAdjuster(Dictionary<string, double> Factors) : ITieredAccrualLedgerFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(TieredAccrualLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAccrualLedgerQuantityOffsetAdjuster(double Weight) : ITieredAccrualLedgerFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(TieredAccrualLedger item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record CapTieredAccrualLedgerAdjuster(double Cap) : ITieredAccrualLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredAccrualLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTieredAccrualLedgerAdjuster(double Step) : ITieredAccrualLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredAccrualLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentTieredAccrualLedgerAdjuster(double Percent) : ITieredAccrualLedgerFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredAccrualLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered accrual ledger steps in order and keeps a trace of each.</summary>
public sealed class TieredAccrualLedgerEngine(List<ITieredAccrualLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredAccrualLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredAccrualLedgerEngine DefaultTieredAccrualLedgerEngine()
    {
        return new TieredAccrualLedgerEngine(
            new List<ITieredAccrualLedgerFactor>
            {
                new LocaleTieredAccrualLedgerAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.8, ["west"] = 1.36, ["retail"] = 0.92 }),
                new TieredAccrualLedgerQuantityOffsetAdjuster(1.52),
                new CapTieredAccrualLedgerAdjuster(13864.0),
                new RoundingTieredAccrualLedgerAdjuster(0.5),
                new PercentTieredAccrualLedgerAdjuster(-22.4),
            },
            new List<string>(),
            1.0,
            97029.9);
    }

    public double Evaluate(TieredAccrualLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredAccrualLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
