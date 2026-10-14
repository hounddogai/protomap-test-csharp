namespace Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>One step of the strict sentiment quota calculation.</summary>
public interface IStrictSentimentQuotaStep
{
    string Describe();
    double Adjust(StrictSentimentQuota item, double valueValue);
}

public sealed record MinimumStrictSentimentQuotaAdjuster(double Minimum) : IStrictSentimentQuotaStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictSentimentQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredStrictSentimentQuotaAdjuster(List<double> Bounds, List<double> Rates) : IStrictSentimentQuotaStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictSentimentQuota item, double valueValue)
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

public sealed record RegionStrictSentimentQuotaAdjuster(Dictionary<string, double> Factors) : IStrictSentimentQuotaStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(StrictSentimentQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict sentiment quota steps in order and keeps a trace of each.</summary>
public sealed class StrictSentimentQuotaEngine(List<IStrictSentimentQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictSentimentQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictSentimentQuotaEngine DefaultStrictSentimentQuotaEngine()
    {
        return new StrictSentimentQuotaEngine(
            new List<IStrictSentimentQuotaStep>
            {
                new MinimumStrictSentimentQuotaAdjuster(25.3),
                new TieredStrictSentimentQuotaAdjuster(
                    new List<double> { 6014.0, 8141.0 },
                    new List<double> { 1.1, 0.92 }),
                new RegionStrictSentimentQuotaAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.81, ["central"] = 0.96, ["online"] = 1.37 }),
            },
            new List<string>(),
            2.7,
            10404.7);
    }

    public double Run(StrictSentimentQuota item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictSentimentQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
