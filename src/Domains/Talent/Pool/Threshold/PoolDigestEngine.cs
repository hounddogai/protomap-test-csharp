namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool digest calculation.</summary>
public interface IPoolDigestModifier
{
    string Describe();
    double Adjust(PoolDigest item, double valueValue);
}

public sealed record CapPoolDigestAdjuster(double Cap) : IPoolDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PoolDigestSequenceOffsetAdjuster(double Weight) : IPoolDigestModifier
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(PoolDigest item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record RoundingPoolDigestAdjuster(double Step) : IPoolDigestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pool digest steps in order and keeps a trace of each.</summary>
public sealed class PoolDigestEngine(List<IPoolDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolDigestEngine DefaultPoolDigestEngine()
    {
        return new PoolDigestEngine(
            new List<IPoolDigestModifier>
            {
                new CapPoolDigestAdjuster(5033.0),
                new PoolDigestSequenceOffsetAdjuster(0.37),
                new RoundingPoolDigestAdjuster(5.0),
            },
            new List<string>(),
            2.8,
            18005.9);
    }

    public double Evaluate(PoolDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<PoolDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
