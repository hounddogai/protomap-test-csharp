namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>One step of the sentiment plan calculation.</summary>
public interface ISentimentPlanFactor
{
    string Describe();
    double Adjust(SentimentPlan item, double valueValue);
}

public sealed record CapSentimentPlanAdjuster(double Cap) : ISentimentPlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CategorySentimentPlanAdjuster(Dictionary<string, double> Factors) : ISentimentPlanFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(SentimentPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSentimentPlanAdjuster(double Percent) : ISentimentPlanFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSentimentPlanAdjuster(List<double> Bounds, List<double> Rates) : ISentimentPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentPlan item, double valueValue)
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

public sealed record MinimumSentimentPlanAdjuster(double Minimum) : ISentimentPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the sentiment plan steps in order and keeps a trace of each.</summary>
public sealed class SentimentPlanEngine(List<ISentimentPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentPlanEngine DefaultSentimentPlanEngine()
    {
        return new SentimentPlanEngine(
            new List<ISentimentPlanFactor>
            {
                new CapSentimentPlanAdjuster(26082.0),
                new CategorySentimentPlanAdjuster(new Dictionary<string, double> { ["central"] = 1.37 }),
                new PercentSentimentPlanAdjuster(-0.4),
                new TieredSentimentPlanAdjuster(new List<double> { 526.0, 5490.0 }, new List<double> { 1.09, 0.89 }),
                new MinimumSentimentPlanAdjuster(47.9),
            },
            new List<string>(),
            3.0,
            18955.1);
    }

    public double Compute(SentimentPlan item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
