namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered sentiment adjustment calculation.</summary>
public interface ITieredSentimentAdjustmentStep
{
    string Describe();
    double Adjust(TieredSentimentAdjustment item, double valueValue);
}

public sealed record TieredTieredSentimentAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : ITieredSentimentAdjustmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredSentimentAdjustment item, double valueValue)
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

public sealed record PercentTieredSentimentAdjustmentAdjuster(double Percent) : ITieredSentimentAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredSentimentAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredSentimentAdjustmentAdjuster(double Step) : ITieredSentimentAdjustmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredSentimentAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelTieredSentimentAdjustmentAdjuster(Dictionary<string, double> Factors) : ITieredSentimentAdjustmentStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TieredSentimentAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumTieredSentimentAdjustmentAdjuster(double Minimum) : ITieredSentimentAdjustmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredSentimentAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered sentiment adjustment steps in order and keeps a trace of each.</summary>
public sealed class TieredSentimentAdjustmentEngine(List<ITieredSentimentAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredSentimentAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredSentimentAdjustmentEngine DefaultTieredSentimentAdjustmentEngine()
    {
        return new TieredSentimentAdjustmentEngine(
            new List<ITieredSentimentAdjustmentStep>
            {
                new TieredTieredSentimentAdjustmentAdjuster(
                    new List<double> { 2223.0, 4272.0, 5908.0, 7898.0 },
                    new List<double> { 0.88, 0.87, 0.98, 1.03 }),
                new PercentTieredSentimentAdjustmentAdjuster(-6.6),
                new RoundingTieredSentimentAdjustmentAdjuster(0.1),
                new LabelTieredSentimentAdjustmentAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.92, ["central"] = 0.91 }),
                new MinimumTieredSentimentAdjustmentAdjuster(9.6),
            },
            new List<string>(),
            4.0,
            2467.3);
    }

    public double Run(TieredSentimentAdjustment item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredSentimentAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
