namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool target calculation.</summary>
public interface IPoolTargetAdjuster
{
    string Describe();
    double Adjust(PoolTarget item, double valueValue);
}

public sealed record PercentPoolTargetAdjuster(double Percent) : IPoolTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PoolTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPoolTargetAdjuster(double Cap) : IPoolTargetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPoolTargetAdjuster(double Step) : IPoolTargetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPoolTargetAdjuster(double Minimum) : IPoolTargetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pool target steps in order and keeps a trace of each.</summary>
public sealed class PoolTargetEngine(List<IPoolTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolTargetEngine DefaultPoolTargetEngine()
    {
        return new PoolTargetEngine(
            new List<IPoolTargetAdjuster>
            {
                new PercentPoolTargetAdjuster(-13.4),
                new CapPoolTargetAdjuster(30092.0),
                new RoundingPoolTargetAdjuster(5.0),
                new MinimumPoolTargetAdjuster(38.9),
            },
            new List<string>(),
            3.6,
            39423.5);
    }

    public double Price(PoolTarget item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PoolTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
