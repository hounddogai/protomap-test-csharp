namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant metric calculation.</summary>
public interface IEquityGrantMetricFactor
{
    string Describe();
    double Adjust(EquityGrantMetric item, double valueValue);
}

public sealed record RoundingEquityGrantMetricAdjuster(double Step) : IEquityGrantMetricFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DepartmentEquityGrantMetricAdjuster(Dictionary<string, double> Factors) : IEquityGrantMetricFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(EquityGrantMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredEquityGrantMetricAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantMetric item, double valueValue)
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

public sealed record MinimumEquityGrantMetricAdjuster(double Minimum) : IEquityGrantMetricFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the equity grant metric steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantMetricEngine(List<IEquityGrantMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantMetricEngine DefaultEquityGrantMetricEngine()
    {
        return new EquityGrantMetricEngine(
            new List<IEquityGrantMetricFactor>
            {
                new RoundingEquityGrantMetricAdjuster(0.25),
                new DepartmentEquityGrantMetricAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.13, ["south"] = 0.89 }),
                new TieredEquityGrantMetricAdjuster(
                    new List<double> { 4746.0, 4988.0, 7015.0, 9348.0 },
                    new List<double> { 0.97, 0.92, 1.1, 1.13 }),
                new MinimumEquityGrantMetricAdjuster(15.3),
            },
            new List<string>(),
            3.1,
            57495.8);
    }

    public double Price(EquityGrantMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
