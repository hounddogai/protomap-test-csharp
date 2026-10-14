namespace Acme.Workforce.Compensation.Adjustment.Tier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the batched adjustment rule calculation.</summary>
public interface IBatchedAdjustmentRuleFactor
{
    string Describe();
    double Adjust(BatchedAdjustmentRule item, double valueValue);
}

public sealed record TieredBatchedAdjustmentRuleAdjuster(List<double> Bounds, List<double> Rates) : IBatchedAdjustmentRuleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedAdjustmentRule item, double valueValue)
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

public sealed record MinimumBatchedAdjustmentRuleAdjuster(double Minimum) : IBatchedAdjustmentRuleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedAdjustmentRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBatchedAdjustmentRuleAdjuster(double Cap) : IBatchedAdjustmentRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedAdjustmentRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBatchedAdjustmentRuleAdjuster(Registry Flags, string Flag, double Percent) : IBatchedAdjustmentRuleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedAdjustmentRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched adjustment rule steps in order and keeps a trace of each.</summary>
public sealed class BatchedAdjustmentRuleEngine(List<IBatchedAdjustmentRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedAdjustmentRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedAdjustmentRuleEngine DefaultBatchedAdjustmentRuleEngine(Registry flags)
    {
        return new BatchedAdjustmentRuleEngine(
            new List<IBatchedAdjustmentRuleFactor>
            {
                new TieredBatchedAdjustmentRuleAdjuster(
                    new List<double> { 2070.0, 2907.0 },
                    new List<double> { 0.88, 1.05 }),
                new MinimumBatchedAdjustmentRuleAdjuster(12.4),
                new CapBatchedAdjustmentRuleAdjuster(27944.0),
                new FlaggedBatchedAdjustmentRuleAdjuster(flags, "batched-adjustment-rule-default", 7.1),
            },
            new List<string>(),
            2.8,
            62533.2);
    }

    public double Price(BatchedAdjustmentRule item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedAdjustmentRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
