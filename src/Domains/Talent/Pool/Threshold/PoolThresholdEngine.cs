namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool threshold calculation.</summary>
public interface IPoolThresholdModifier
{
    string Describe();
    double Adjust(PoolThreshold item, double valueValue);
}

public sealed record RoundingPoolThresholdAdjuster(double Step) : IPoolThresholdModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPoolThresholdAdjuster(double Minimum) : IPoolThresholdModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DepartmentPoolThresholdAdjuster(Dictionary<string, double> Factors) : IPoolThresholdModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PoolThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPoolThresholdAdjuster(double Cap) : IPoolThresholdModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pool threshold steps in order and keeps a trace of each.</summary>
public sealed class PoolThresholdEngine(List<IPoolThresholdModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolThresholdModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolThresholdEngine DefaultPoolThresholdEngine()
    {
        return new PoolThresholdEngine(
            new List<IPoolThresholdModifier>
            {
                new RoundingPoolThresholdAdjuster(0.1),
                new MinimumPoolThresholdAdjuster(43.0),
                new DepartmentPoolThresholdAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.29, ["online"] = 0.85 }),
                new CapPoolThresholdAdjuster(45283.0),
            },
            new List<string>(),
            1.8,
            88963.0);
    }

    public double Compute(PoolThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<PoolThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
