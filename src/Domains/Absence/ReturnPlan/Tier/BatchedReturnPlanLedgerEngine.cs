namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched return plan ledger calculation.</summary>
public interface IBatchedReturnPlanLedgerStep
{
    string Describe();
    double Adjust(BatchedReturnPlanLedger item, double valueValue);
}

public sealed record CapBatchedReturnPlanLedgerAdjuster(double Cap) : IBatchedReturnPlanLedgerStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedReturnPlanLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBatchedReturnPlanLedgerAdjuster(double Step) : IBatchedReturnPlanLedgerStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedReturnPlanLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumBatchedReturnPlanLedgerAdjuster(double Minimum) : IBatchedReturnPlanLedgerStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedReturnPlanLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedBatchedReturnPlanLedgerAdjuster(Registry Flags, string Flag, double Percent) : IBatchedReturnPlanLedgerStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedReturnPlanLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBatchedReturnPlanLedgerAdjuster(List<double> Bounds, List<double> Rates) : IBatchedReturnPlanLedgerStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedReturnPlanLedger item, double valueValue)
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

/// <summary>Runs the batched return plan ledger steps in order and keeps a trace of each.</summary>
public sealed class BatchedReturnPlanLedgerEngine(List<IBatchedReturnPlanLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedReturnPlanLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedReturnPlanLedgerEngine DefaultBatchedReturnPlanLedgerEngine(Registry flags)
    {
        return new BatchedReturnPlanLedgerEngine(
            new List<IBatchedReturnPlanLedgerStep>
            {
                new CapBatchedReturnPlanLedgerAdjuster(7727.0),
                new RoundingBatchedReturnPlanLedgerAdjuster(0.5),
                new MinimumBatchedReturnPlanLedgerAdjuster(5.7),
                new FlaggedBatchedReturnPlanLedgerAdjuster(flags, "batched-return-plan-ledger-strict", -6.5),
                new TieredBatchedReturnPlanLedgerAdjuster(
                    new List<double> { 1251.0, 1601.0, 4287.0, 6332.0 },
                    new List<double> { 1.14, 0.92, 0.9, 0.81 }),
            },
            new List<string>(),
            1.8,
            20278.0);
    }

    public double Evaluate(BatchedReturnPlanLedger item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedReturnPlanLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
