namespace Acme.Workforce.Contractors.Engagement.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched engagement exception calculation.</summary>
public interface IBatchedEngagementExceptionFactor
{
    string Describe();
    double Adjust(BatchedEngagementException item, double valueValue);
}

public sealed record RoundingBatchedEngagementExceptionAdjuster(double Step) : IBatchedEngagementExceptionFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedEngagementException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record BatchedEngagementExceptionMarginOffsetAdjuster(double Weight) : IBatchedEngagementExceptionFactor
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(BatchedEngagementException item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record CapBatchedEngagementExceptionAdjuster(double Cap) : IBatchedEngagementExceptionFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedEngagementException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the batched engagement exception steps in order and keeps a trace of each.</summary>
public sealed class BatchedEngagementExceptionEngine(List<IBatchedEngagementExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedEngagementExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedEngagementExceptionEngine DefaultBatchedEngagementExceptionEngine()
    {
        return new BatchedEngagementExceptionEngine(
            new List<IBatchedEngagementExceptionFactor>
            {
                new RoundingBatchedEngagementExceptionAdjuster(5.0),
                new BatchedEngagementExceptionMarginOffsetAdjuster(0.88),
                new CapBatchedEngagementExceptionAdjuster(39689.0),
            },
            new List<string>(),
            3.3,
            27195.3);
    }

    public double Evaluate(BatchedEngagementException item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedEngagementException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
