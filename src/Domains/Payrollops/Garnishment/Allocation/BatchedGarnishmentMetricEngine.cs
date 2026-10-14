namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched garnishment metric calculation.</summary>
public interface IBatchedGarnishmentMetricModifier
{
    string Describe();
    double Adjust(BatchedGarnishmentMetric item, double valueValue);
}

public sealed record MinimumBatchedGarnishmentMetricAdjuster(double Minimum) : IBatchedGarnishmentMetricModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedGarnishmentMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBatchedGarnishmentMetricAdjuster(double Cap) : IBatchedGarnishmentMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedGarnishmentMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBatchedGarnishmentMetricAdjuster(double Step) : IBatchedGarnishmentMetricModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedGarnishmentMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBatchedGarnishmentMetricAdjuster(double Percent) : IBatchedGarnishmentMetricModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedGarnishmentMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched garnishment metric steps in order and keeps a trace of each.</summary>
public sealed class BatchedGarnishmentMetricEngine(List<IBatchedGarnishmentMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedGarnishmentMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedGarnishmentMetricEngine DefaultBatchedGarnishmentMetricEngine()
    {
        return new BatchedGarnishmentMetricEngine(
            new List<IBatchedGarnishmentMetricModifier>
            {
                new MinimumBatchedGarnishmentMetricAdjuster(33.7),
                new CapBatchedGarnishmentMetricAdjuster(6391.0),
                new RoundingBatchedGarnishmentMetricAdjuster(1.0),
                new PercentBatchedGarnishmentMetricAdjuster(6.5),
            },
            new List<string>(),
            3.4,
            83578.1);
    }

    public double Run(BatchedGarnishmentMetric item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedGarnishmentMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
