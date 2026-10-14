namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor batch calculation.</summary>
public interface IVisitorBatchStep
{
    string Describe();
    double Adjust(VisitorBatch item, double valueValue);
}

public sealed record CapVisitorBatchAdjuster(double Cap) : IVisitorBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentVisitorBatchAdjuster(double Percent) : IVisitorBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredVisitorBatchAdjuster(List<double> Bounds, List<double> Rates) : IVisitorBatchStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorBatch item, double valueValue)
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

public sealed record MinimumVisitorBatchAdjuster(double Minimum) : IVisitorBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record VisitorBatchRateOffsetAdjuster(double Weight) : IVisitorBatchStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(VisitorBatch item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the visitor batch steps in order and keeps a trace of each.</summary>
public sealed class VisitorBatchEngine(List<IVisitorBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorBatchEngine DefaultVisitorBatchEngine()
    {
        return new VisitorBatchEngine(
            new List<IVisitorBatchStep>
            {
                new CapVisitorBatchAdjuster(48442.0),
                new PercentVisitorBatchAdjuster(-19.5),
                new TieredVisitorBatchAdjuster(new List<double> { 6999.0, 9915.0 }, new List<double> { 0.96, 0.95 }),
                new MinimumVisitorBatchAdjuster(21.0),
                new VisitorBatchRateOffsetAdjuster(0.22),
            },
            new List<string>(),
            2.6,
            19009.6);
    }

    public double Evaluate(VisitorBatch item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
