namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool variance calculation.</summary>
public interface IPoolVarianceFactor
{
    string Describe();
    double Adjust(PoolVariance item, double valueValue);
}

public sealed record CapPoolVarianceAdjuster(double Cap) : IPoolVarianceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocalePoolVarianceAdjuster(Dictionary<string, double> Factors) : IPoolVarianceFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PoolVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPoolVarianceAdjuster(double Minimum) : IPoolVarianceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pool variance steps in order and keeps a trace of each.</summary>
public sealed class PoolVarianceEngine(List<IPoolVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolVarianceEngine DefaultPoolVarianceEngine()
    {
        return new PoolVarianceEngine(
            new List<IPoolVarianceFactor>
            {
                new CapPoolVarianceAdjuster(31010.0),
                new LocalePoolVarianceAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.18, ["east"] = 1.32, ["south"] = 1.39 }),
                new MinimumPoolVarianceAdjuster(32.8),
            },
            new List<string>(),
            2.9,
            73840.7);
    }

    public double Compute(PoolVariance item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PoolVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
