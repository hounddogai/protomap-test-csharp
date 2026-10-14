namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool exception calculation.</summary>
public interface IPoolExceptionStep
{
    string Describe();
    double Adjust(PoolException item, double valueValue);
}

public sealed record PoolExceptionQuantityOffsetAdjuster(double Weight) : IPoolExceptionStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(PoolException item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record FlaggedPoolExceptionAdjuster(Registry Flags, string Flag, double Percent) : IPoolExceptionStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPoolExceptionAdjuster(double Step) : IPoolExceptionStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPoolExceptionAdjuster(double Percent) : IPoolExceptionStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PoolException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pool exception steps in order and keeps a trace of each.</summary>
public sealed class PoolExceptionEngine(List<IPoolExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolExceptionEngine DefaultPoolExceptionEngine(Registry flags)
    {
        return new PoolExceptionEngine(
            new List<IPoolExceptionStep>
            {
                new PoolExceptionQuantityOffsetAdjuster(1.27),
                new FlaggedPoolExceptionAdjuster(flags, "pool-exception-manual", -8.0),
                new RoundingPoolExceptionAdjuster(0.1),
                new PercentPoolExceptionAdjuster(22.6),
            },
            new List<string>(),
            3.6,
            77257.5);
    }

    public double Price(PoolException item)
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

    public Dictionary<string, double> EvaluateAll(List<PoolException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
