namespace Acme.Workforce.Compensation.EquityGrant.Batch;

/// <summary>One step of the equity grant batch calculation.</summary>
public interface IEquityGrantBatchModifier
{
    string Describe();
    double Adjust(EquityGrantBatch item, double valueValue);
}

public sealed record CapEquityGrantBatchAdjuster(double Cap) : IEquityGrantBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record EquityGrantBatchWeightOffsetAdjuster(double Weight) : IEquityGrantBatchModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(EquityGrantBatch item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record TitleEquityGrantBatchAdjuster(Dictionary<string, double> Factors) : IEquityGrantBatchModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(EquityGrantBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the equity grant batch steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantBatchEngine(List<IEquityGrantBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantBatchEngine DefaultEquityGrantBatchEngine()
    {
        return new EquityGrantBatchEngine(
            new List<IEquityGrantBatchModifier>
            {
                new CapEquityGrantBatchAdjuster(4807.0),
                new EquityGrantBatchWeightOffsetAdjuster(0.9),
                new TitleEquityGrantBatchAdjuster(new Dictionary<string, double> { ["east"] = 1.39 }),
            },
            new List<string>(),
            1.3,
            23432.5);
    }

    public double Run(EquityGrantBatch item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
