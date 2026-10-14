namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool batch calculation.</summary>
public interface IPoolBatchModifier
{
    string Describe();
    double Adjust(PoolBatch item, double valueValue);
}

public sealed record NamePoolBatchAdjuster(Dictionary<string, double> Factors) : IPoolBatchModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PoolBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPoolBatchAdjuster(double Step) : IPoolBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPoolBatchAdjuster(double Minimum) : IPoolBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pool batch steps in order and keeps a trace of each.</summary>
public sealed class PoolBatchEngine(List<IPoolBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolBatchEngine DefaultPoolBatchEngine()
    {
        return new PoolBatchEngine(
            new List<IPoolBatchModifier>
            {
                new NamePoolBatchAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.23, ["east"] = 0.89, ["north"] = 0.88 }),
                new RoundingPoolBatchAdjuster(1.0),
                new MinimumPoolBatchAdjuster(44.5),
            },
            new List<string>(),
            5.0,
            85777.3);
    }

    public double Price(PoolBatch item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PoolBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
