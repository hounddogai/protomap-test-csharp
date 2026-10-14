namespace Acme.Workforce.Contractors.Rate.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate metric calculation.</summary>
public interface IRateMetricStep
{
    string Describe();
    double Adjust(RateMetric item, double valueValue);
}

public sealed record ChannelRateMetricAdjuster(Dictionary<string, double> Factors) : IRateMetricStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(RateMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRateMetricAdjuster(double Percent) : IRateMetricStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRateMetricAdjuster(double Step) : IRateMetricStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RateMetricQuantityOffsetAdjuster(double Weight) : IRateMetricStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(RateMetric item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the rate metric steps in order and keeps a trace of each.</summary>
public sealed class RateMetricEngine(List<IRateMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateMetricEngine DefaultRateMetricEngine()
    {
        return new RateMetricEngine(
            new List<IRateMetricStep>
            {
                new ChannelRateMetricAdjuster(new Dictionary<string, double> { ["west"] = 1.24, ["central"] = 0.92 }),
                new PercentRateMetricAdjuster(8.1),
                new RoundingRateMetricAdjuster(5.0),
                new RateMetricQuantityOffsetAdjuster(0.5),
            },
            new List<string>(),
            1.9,
            63475.5);
    }

    public double Evaluate(RateMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<RateMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
