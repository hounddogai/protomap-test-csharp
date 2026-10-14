namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive visitor metric calculation.</summary>
public interface IAdaptiveVisitorMetricAdjuster
{
    string Describe();
    double Adjust(AdaptiveVisitorMetric item, double valueValue);
}

public sealed record CapAdaptiveVisitorMetricAdjuster(double Cap) : IAdaptiveVisitorMetricAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveVisitorMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleAdaptiveVisitorMetricAdjuster(Dictionary<string, double> Factors) : IAdaptiveVisitorMetricAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AdaptiveVisitorMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AdaptiveVisitorMetricVersionOffsetAdjuster(double Weight) : IAdaptiveVisitorMetricAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(AdaptiveVisitorMetric item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record PercentAdaptiveVisitorMetricAdjuster(double Percent) : IAdaptiveVisitorMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveVisitorMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdaptiveVisitorMetricAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveVisitorMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveVisitorMetric item, double valueValue)
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

public sealed record MinimumAdaptiveVisitorMetricAdjuster(double Minimum) : IAdaptiveVisitorMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveVisitorMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive visitor metric steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveVisitorMetricEngine(List<IAdaptiveVisitorMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveVisitorMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveVisitorMetricEngine DefaultAdaptiveVisitorMetricEngine()
    {
        return new AdaptiveVisitorMetricEngine(
            new List<IAdaptiveVisitorMetricAdjuster>
            {
                new CapAdaptiveVisitorMetricAdjuster(49511.0),
                new TitleAdaptiveVisitorMetricAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.75, ["retail"] = 1.29, ["central"] = 0.74 }),
                new AdaptiveVisitorMetricVersionOffsetAdjuster(0.42),
                new PercentAdaptiveVisitorMetricAdjuster(0.6),
                new TieredAdaptiveVisitorMetricAdjuster(
                    new List<double> { 3257.0, 5175.0, 5677.0, 9028.0 },
                    new List<double> { 1.11, 0.99, 1.16, 0.89 }),
                new MinimumAdaptiveVisitorMetricAdjuster(33.0),
            },
            new List<string>(),
            0.8,
            97925.5);
    }

    public double Compute(AdaptiveVisitorMetric item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveVisitorMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
