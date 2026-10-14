namespace Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>One step of the batched warranty ledger calculation.</summary>
public interface IBatchedWarrantyLedgerFactor
{
    string Describe();
    double Adjust(BatchedWarrantyLedger item, double valueValue);
}

public sealed record CapBatchedWarrantyLedgerAdjuster(double Cap) : IBatchedWarrantyLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedWarrantyLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record BatchedWarrantyLedgerCostOffsetAdjuster(double Weight) : IBatchedWarrantyLedgerFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(BatchedWarrantyLedger item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record PercentBatchedWarrantyLedgerAdjuster(double Percent) : IBatchedWarrantyLedgerFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedWarrantyLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBatchedWarrantyLedgerAdjuster(double Minimum) : IBatchedWarrantyLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedWarrantyLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredBatchedWarrantyLedgerAdjuster(List<double> Bounds, List<double> Rates) : IBatchedWarrantyLedgerFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedWarrantyLedger item, double valueValue)
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

/// <summary>Runs the batched warranty ledger steps in order and keeps a trace of each.</summary>
public sealed class BatchedWarrantyLedgerEngine(List<IBatchedWarrantyLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedWarrantyLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedWarrantyLedgerEngine DefaultBatchedWarrantyLedgerEngine()
    {
        return new BatchedWarrantyLedgerEngine(
            new List<IBatchedWarrantyLedgerFactor>
            {
                new CapBatchedWarrantyLedgerAdjuster(23240.0),
                new BatchedWarrantyLedgerCostOffsetAdjuster(1.99),
                new PercentBatchedWarrantyLedgerAdjuster(16.2),
                new MinimumBatchedWarrantyLedgerAdjuster(24.7),
                new TieredBatchedWarrantyLedgerAdjuster(
                    new List<double> { 594.0, 2356.0, 8310.0, 8595.0 },
                    new List<double> { 0.84, 0.98, 0.85, 1.18 }),
            },
            new List<string>(),
            4.8,
            60113.1);
    }

    public double Price(BatchedWarrantyLedger item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedWarrantyLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
