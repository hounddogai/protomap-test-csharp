namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched asset return budget calculation.</summary>
public interface IBatchedAssetReturnBudgetAdjuster
{
    string Describe();
    double Adjust(BatchedAssetReturnBudget item, double valueValue);
}

public sealed record MinimumBatchedAssetReturnBudgetAdjuster(double Minimum) : IBatchedAssetReturnBudgetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedAssetReturnBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedBatchedAssetReturnBudgetAdjuster(Registry Flags, string Flag, double Percent) : IBatchedAssetReturnBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedAssetReturnBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBatchedAssetReturnBudgetAdjuster(double Step) : IBatchedAssetReturnBudgetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedAssetReturnBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the batched asset return budget steps in order and keeps a trace of each.</summary>
public sealed class BatchedAssetReturnBudgetEngine(List<IBatchedAssetReturnBudgetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedAssetReturnBudgetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedAssetReturnBudgetEngine DefaultBatchedAssetReturnBudgetEngine(Registry flags)
    {
        return new BatchedAssetReturnBudgetEngine(
            new List<IBatchedAssetReturnBudgetAdjuster>
            {
                new MinimumBatchedAssetReturnBudgetAdjuster(41.8),
                new FlaggedBatchedAssetReturnBudgetAdjuster(flags, "batched-asset-return-budget-tiered", 1.3),
                new RoundingBatchedAssetReturnBudgetAdjuster(1.0),
            },
            new List<string>(),
            0.1,
            60730.3);
    }

    public double Evaluate(BatchedAssetReturnBudget item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedAssetReturnBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
