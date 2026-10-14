namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor metric calculation.</summary>
public interface IVisitorMetricStep
{
    string Describe();
    double Adjust(VisitorMetric item, double valueValue);
}

public sealed record TieredVisitorMetricAdjuster(List<double> Bounds, List<double> Rates) : IVisitorMetricStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorMetric item, double valueValue)
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

public sealed record MinimumVisitorMetricAdjuster(double Minimum) : IVisitorMetricStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record VisitorMetricScoreOffsetAdjuster(double Weight) : IVisitorMetricStep
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(VisitorMetric item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record PercentVisitorMetricAdjuster(double Percent) : IVisitorMetricStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingVisitorMetricAdjuster(double Step) : IVisitorMetricStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the visitor metric steps in order and keeps a trace of each.</summary>
public sealed class VisitorMetricEngine(List<IVisitorMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorMetricEngine DefaultVisitorMetricEngine()
    {
        return new VisitorMetricEngine(
            new List<IVisitorMetricStep>
            {
                new TieredVisitorMetricAdjuster(
                    new List<double> { 897.0, 4814.0, 5422.0, 9091.0 },
                    new List<double> { 0.96, 0.98, 0.99, 0.86 }),
                new MinimumVisitorMetricAdjuster(27.7),
                new VisitorMetricScoreOffsetAdjuster(1.32),
                new PercentVisitorMetricAdjuster(8.6),
                new RoundingVisitorMetricAdjuster(0.1),
            },
            new List<string>(),
            1.3,
            30812.4);
    }

    public double Price(VisitorMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<VisitorMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
