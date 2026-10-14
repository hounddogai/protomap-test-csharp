namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched action plan bundle calculation.</summary>
public interface IBatchedActionPlanBundleAdjuster
{
    string Describe();
    double Adjust(BatchedActionPlanBundle item, double valueValue);
}

public sealed record RoundingBatchedActionPlanBundleAdjuster(double Step) : IBatchedActionPlanBundleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedActionPlanBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredBatchedActionPlanBundleAdjuster(List<double> Bounds, List<double> Rates) : IBatchedActionPlanBundleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedActionPlanBundle item, double valueValue)
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

public sealed record MinimumBatchedActionPlanBundleAdjuster(double Minimum) : IBatchedActionPlanBundleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedActionPlanBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBatchedActionPlanBundleAdjuster(double Cap) : IBatchedActionPlanBundleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedActionPlanBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CategoryBatchedActionPlanBundleAdjuster(Dictionary<string, double> Factors) : IBatchedActionPlanBundleAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(BatchedActionPlanBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentBatchedActionPlanBundleAdjuster(double Percent) : IBatchedActionPlanBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedActionPlanBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched action plan bundle steps in order and keeps a trace of each.</summary>
public sealed class BatchedActionPlanBundleEngine(List<IBatchedActionPlanBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedActionPlanBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedActionPlanBundleEngine DefaultBatchedActionPlanBundleEngine()
    {
        return new BatchedActionPlanBundleEngine(
            new List<IBatchedActionPlanBundleAdjuster>
            {
                new RoundingBatchedActionPlanBundleAdjuster(0.5),
                new TieredBatchedActionPlanBundleAdjuster(
                    new List<double> { 4417.0, 7290.0, 9341.0 },
                    new List<double> { 0.96, 0.88, 1.03 }),
                new MinimumBatchedActionPlanBundleAdjuster(1.1),
                new CapBatchedActionPlanBundleAdjuster(8866.0),
                new CategoryBatchedActionPlanBundleAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.77, ["east"] = 1.13, ["south"] = 0.78 }),
                new PercentBatchedActionPlanBundleAdjuster(-11.7),
            },
            new List<string>(),
            4.5,
            95000.9);
    }

    public double Price(BatchedActionPlanBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedActionPlanBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
