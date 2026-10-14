namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling equity grant metric calculation.</summary>
public interface IRollingEquityGrantMetricFactor
{
    string Describe();
    double Adjust(RollingEquityGrantMetric item, double valueValue);
}

public sealed record PercentRollingEquityGrantMetricAdjuster(double Percent) : IRollingEquityGrantMetricFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingEquityGrantMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceRollingEquityGrantMetricAdjuster(Dictionary<string, double> Factors) : IRollingEquityGrantMetricFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(RollingEquityGrantMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRollingEquityGrantMetricAdjuster(List<double> Bounds, List<double> Rates) : IRollingEquityGrantMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingEquityGrantMetric item, double valueValue)
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

public sealed record RollingEquityGrantMetricRateOffsetAdjuster(double Weight) : IRollingEquityGrantMetricFactor
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(RollingEquityGrantMetric item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the rolling equity grant metric steps in order and keeps a trace of each.</summary>
public sealed class RollingEquityGrantMetricEngine(List<IRollingEquityGrantMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingEquityGrantMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingEquityGrantMetricEngine DefaultRollingEquityGrantMetricEngine()
    {
        return new RollingEquityGrantMetricEngine(
            new List<IRollingEquityGrantMetricFactor>
            {
                new PercentRollingEquityGrantMetricAdjuster(23.5),
                new SourceRollingEquityGrantMetricAdjuster(new Dictionary<string, double> { ["west"] = 1.14 }),
                new TieredRollingEquityGrantMetricAdjuster(
                    new List<double> { 1189.0, 8121.0 },
                    new List<double> { 0.99, 1.08 }),
                new RollingEquityGrantMetricRateOffsetAdjuster(1.6),
            },
            new List<string>(),
            0.2,
            34585.8);
    }

    public double Compute(RollingEquityGrantMetric item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RollingEquityGrantMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
