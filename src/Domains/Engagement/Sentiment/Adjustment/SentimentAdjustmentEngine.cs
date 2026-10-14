namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment adjustment calculation.</summary>
public interface ISentimentAdjustmentFactor
{
    string Describe();
    double Adjust(SentimentAdjustment item, double valueValue);
}

public sealed record MinimumSentimentAdjustmentAdjuster(double Minimum) : ISentimentAdjustmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SentimentAdjustmentWeightOffsetAdjuster(double Weight) : ISentimentAdjustmentFactor
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(SentimentAdjustment item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record RoundingSentimentAdjustmentAdjuster(double Step) : ISentimentAdjustmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the sentiment adjustment steps in order and keeps a trace of each.</summary>
public sealed class SentimentAdjustmentEngine(List<ISentimentAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentAdjustmentEngine DefaultSentimentAdjustmentEngine()
    {
        return new SentimentAdjustmentEngine(
            new List<ISentimentAdjustmentFactor>
            {
                new MinimumSentimentAdjustmentAdjuster(9.0),
                new SentimentAdjustmentWeightOffsetAdjuster(0.85),
                new RoundingSentimentAdjustmentAdjuster(1.0),
            },
            new List<string>(),
            2.1,
            72614.4);
    }

    public double Evaluate(SentimentAdjustment item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
