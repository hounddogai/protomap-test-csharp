namespace Acme.Workforce.Absence.LeaveRequest.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched leave request forecast calculation.</summary>
public interface IBatchedLeaveRequestForecastAdjuster
{
    string Describe();
    double Adjust(BatchedLeaveRequestForecast item, double valueValue);
}

public sealed record PercentBatchedLeaveRequestForecastAdjuster(double Percent) : IBatchedLeaveRequestForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedLeaveRequestForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BatchedLeaveRequestForecastDiscountOffsetAdjuster(double Weight) : IBatchedLeaveRequestForecastAdjuster
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(BatchedLeaveRequestForecast item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record RoundingBatchedLeaveRequestForecastAdjuster(double Step) : IBatchedLeaveRequestForecastAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedLeaveRequestForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumBatchedLeaveRequestForecastAdjuster(double Minimum) : IBatchedLeaveRequestForecastAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedLeaveRequestForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the batched leave request forecast steps in order and keeps a trace of each.</summary>
public sealed class BatchedLeaveRequestForecastEngine(List<IBatchedLeaveRequestForecastAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedLeaveRequestForecastAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedLeaveRequestForecastEngine DefaultBatchedLeaveRequestForecastEngine()
    {
        return new BatchedLeaveRequestForecastEngine(
            new List<IBatchedLeaveRequestForecastAdjuster>
            {
                new PercentBatchedLeaveRequestForecastAdjuster(-18.2),
                new BatchedLeaveRequestForecastDiscountOffsetAdjuster(0.31),
                new RoundingBatchedLeaveRequestForecastAdjuster(0.1),
                new MinimumBatchedLeaveRequestForecastAdjuster(33.8),
            },
            new List<string>(),
            3.1,
            48712.0);
    }

    public double Evaluate(BatchedLeaveRequestForecast item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedLeaveRequestForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
