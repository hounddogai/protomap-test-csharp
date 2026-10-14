namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool record calculation.</summary>
public interface IPoolRecordFactor
{
    string Describe();
    double Adjust(PoolRecord item, double valueValue);
}

public sealed record TitlePoolRecordAdjuster(Dictionary<string, double> Factors) : IPoolRecordFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PoolRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPoolRecordAdjuster(double Step) : IPoolRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPoolRecordAdjuster(List<double> Bounds, List<double> Rates) : IPoolRecordFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolRecord item, double valueValue)
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

public sealed record CapPoolRecordAdjuster(double Cap) : IPoolRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPoolRecordAdjuster(double Minimum) : IPoolRecordFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pool record steps in order and keeps a trace of each.</summary>
public sealed class PoolRecordEngine(List<IPoolRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolRecordEngine DefaultPoolRecordEngine()
    {
        return new PoolRecordEngine(
            new List<IPoolRecordFactor>
            {
                new TitlePoolRecordAdjuster(new Dictionary<string, double> { ["south"] = 0.8 }),
                new RoundingPoolRecordAdjuster(0.1),
                new TieredPoolRecordAdjuster(new List<double> { 533.0, 1530.0 }, new List<double> { 1.07, 0.91 }),
                new CapPoolRecordAdjuster(5094.0),
                new MinimumPoolRecordAdjuster(1.2),
            },
            new List<string>(),
            2.2,
            63741.5);
    }

    public double Run(PoolRecord item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PoolRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
