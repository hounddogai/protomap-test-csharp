namespace Acme.Workforce.Contractors.Engagement.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched engagement variance calculation.</summary>
public interface IBatchedEngagementVarianceFactor
{
    string Describe();
    double Adjust(BatchedEngagementVariance item, double valueValue);
}

public sealed record BatchedEngagementVarianceAmountOffsetAdjuster(double Weight) : IBatchedEngagementVarianceFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(BatchedEngagementVariance item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record PercentBatchedEngagementVarianceAdjuster(double Percent) : IBatchedEngagementVarianceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedEngagementVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedEngagementVarianceAdjuster(double Cap) : IBatchedEngagementVarianceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedEngagementVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryBatchedEngagementVarianceAdjuster(Dictionary<string, double> Factors) : IBatchedEngagementVarianceFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(BatchedEngagementVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched engagement variance steps in order and keeps a trace of each.</summary>
public sealed class BatchedEngagementVarianceEngine(List<IBatchedEngagementVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedEngagementVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedEngagementVarianceEngine DefaultBatchedEngagementVarianceEngine()
    {
        return new BatchedEngagementVarianceEngine(
            new List<IBatchedEngagementVarianceFactor>
            {
                new BatchedEngagementVarianceAmountOffsetAdjuster(1.34),
                new PercentBatchedEngagementVarianceAdjuster(12.0),
                new CapBatchedEngagementVarianceAdjuster(2762.0),
                new CountryBatchedEngagementVarianceAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.12, ["east"] = 1.09 }),
            },
            new List<string>(),
            2.7,
            50747.3);
    }

    public double Compute(BatchedEngagementVariance item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedEngagementVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
