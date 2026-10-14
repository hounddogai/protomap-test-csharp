namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan batch calculation.</summary>
public interface ISuccessionPlanBatchAdjuster
{
    string Describe();
    double Adjust(SuccessionPlanBatch item, double valueValue);
}

public sealed record CapSuccessionPlanBatchAdjuster(double Cap) : ISuccessionPlanBatchAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentSuccessionPlanBatchAdjuster(double Percent) : ISuccessionPlanBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionSuccessionPlanBatchAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanBatchAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(SuccessionPlanBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSuccessionPlanBatchAdjuster(double Minimum) : ISuccessionPlanBatchAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SuccessionPlanBatchPriorityOffsetAdjuster(double Weight) : ISuccessionPlanBatchAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(SuccessionPlanBatch item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record RoundingSuccessionPlanBatchAdjuster(double Step) : ISuccessionPlanBatchAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the succession plan batch steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanBatchEngine(List<ISuccessionPlanBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanBatchEngine DefaultSuccessionPlanBatchEngine()
    {
        return new SuccessionPlanBatchEngine(
            new List<ISuccessionPlanBatchAdjuster>
            {
                new CapSuccessionPlanBatchAdjuster(44125.0),
                new PercentSuccessionPlanBatchAdjuster(3.4),
                new RegionSuccessionPlanBatchAdjuster(new Dictionary<string, double> { ["online"] = 0.85 }),
                new MinimumSuccessionPlanBatchAdjuster(7.6),
                new SuccessionPlanBatchPriorityOffsetAdjuster(1.7),
                new RoundingSuccessionPlanBatchAdjuster(0.25),
            },
            new List<string>(),
            0.1,
            41302.1);
    }

    public double Evaluate(SuccessionPlanBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
