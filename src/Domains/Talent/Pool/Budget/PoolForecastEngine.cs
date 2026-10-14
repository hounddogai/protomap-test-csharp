namespace Acme.Workforce.Talent.Pool.Budget;

/// <summary>One step of the pool forecast calculation.</summary>
public interface IPoolForecastModifier
{
    string Describe();
    double Adjust(PoolForecast item, double valueValue);
}

public sealed record CategoryPoolForecastAdjuster(Dictionary<string, double> Factors) : IPoolForecastModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PoolForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPoolForecastAdjuster(double Cap) : IPoolForecastModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPoolForecastAdjuster(double Percent) : IPoolForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PoolForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pool forecast steps in order and keeps a trace of each.</summary>
public sealed class PoolForecastEngine(List<IPoolForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolForecastEngine DefaultPoolForecastEngine()
    {
        return new PoolForecastEngine(
            new List<IPoolForecastModifier>
            {
                new CategoryPoolForecastAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.81, ["retail"] = 1.27 }),
                new CapPoolForecastAdjuster(34607.0),
                new PercentPoolForecastAdjuster(-0.5),
            },
            new List<string>(),
            3.6,
            31712.8);
    }

    public double Run(PoolForecast item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PoolForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
