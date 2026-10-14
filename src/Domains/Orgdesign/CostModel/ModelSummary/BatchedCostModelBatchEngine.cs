namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the batched cost model batch calculation.</summary>
public interface IBatchedCostModelBatchModifier
{
    string Describe();
    double Adjust(BatchedCostModelBatch item, double valueValue);
}

public sealed record CapBatchedCostModelBatchAdjuster(double Cap) : IBatchedCostModelBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedCostModelBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DepartmentBatchedCostModelBatchAdjuster(Dictionary<string, double> Factors) : IBatchedCostModelBatchModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(BatchedCostModelBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBatchedCostModelBatchAdjuster(Registry Flags, string Flag, double Percent) : IBatchedCostModelBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedCostModelBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched cost model batch steps in order and keeps a trace of each.</summary>
public sealed class BatchedCostModelBatchEngine(List<IBatchedCostModelBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedCostModelBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedCostModelBatchEngine DefaultBatchedCostModelBatchEngine(Registry flags)
    {
        return new BatchedCostModelBatchEngine(
            new List<IBatchedCostModelBatchModifier>
            {
                new CapBatchedCostModelBatchAdjuster(44421.0),
                new DepartmentBatchedCostModelBatchAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.23, ["south"] = 1.22, ["west"] = 1.18 }),
                new FlaggedBatchedCostModelBatchAdjuster(flags, "batched-cost-model-batch-rolling", -9.4),
            },
            new List<string>(),
            4.3,
            43251.6);
    }

    public double Price(BatchedCostModelBatch item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedCostModelBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
