namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict equity grant metric calculation.</summary>
public interface IStrictEquityGrantMetricAdjuster
{
    string Describe();
    double Adjust(StrictEquityGrantMetric item, double valueValue);
}

public sealed record StrictEquityGrantMetricExpiresAtOffsetAdjuster(double Weight) : IStrictEquityGrantMetricAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(StrictEquityGrantMetric item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record RoundingStrictEquityGrantMetricAdjuster(double Step) : IStrictEquityGrantMetricAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictEquityGrantMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStrictEquityGrantMetricAdjuster(double Minimum) : IStrictEquityGrantMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictEquityGrantMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the strict equity grant metric steps in order and keeps a trace of each.</summary>
public sealed class StrictEquityGrantMetricEngine(List<IStrictEquityGrantMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictEquityGrantMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictEquityGrantMetricEngine DefaultStrictEquityGrantMetricEngine()
    {
        return new StrictEquityGrantMetricEngine(
            new List<IStrictEquityGrantMetricAdjuster>
            {
                new StrictEquityGrantMetricExpiresAtOffsetAdjuster(0.26),
                new RoundingStrictEquityGrantMetricAdjuster(0.5),
                new MinimumStrictEquityGrantMetricAdjuster(36.1),
            },
            new List<string>(),
            2.1,
            12867.7);
    }

    public double Evaluate(StrictEquityGrantMetric item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictEquityGrantMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
