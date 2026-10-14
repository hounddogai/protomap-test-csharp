namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual sentiment adjustment calculation.</summary>
public interface IManualSentimentAdjustmentModifier
{
    string Describe();
    double Adjust(ManualSentimentAdjustment item, double valueValue);
}

public sealed record TieredManualSentimentAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IManualSentimentAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualSentimentAdjustment item, double valueValue)
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

public sealed record ManualSentimentAdjustmentRatingOffsetAdjuster(double Weight) : IManualSentimentAdjustmentModifier
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(ManualSentimentAdjustment item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record RoundingManualSentimentAdjustmentAdjuster(double Step) : IManualSentimentAdjustmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualSentimentAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentManualSentimentAdjustmentAdjuster(double Percent) : IManualSentimentAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualSentimentAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualSentimentAdjustmentAdjuster(double Cap) : IManualSentimentAdjustmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualSentimentAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the manual sentiment adjustment steps in order and keeps a trace of each.</summary>
public sealed class ManualSentimentAdjustmentEngine(List<IManualSentimentAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualSentimentAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualSentimentAdjustmentEngine DefaultManualSentimentAdjustmentEngine()
    {
        return new ManualSentimentAdjustmentEngine(
            new List<IManualSentimentAdjustmentModifier>
            {
                new TieredManualSentimentAdjustmentAdjuster(
                    new List<double> { 188.0, 7861.0, 9971.0 },
                    new List<double> { 1.05, 1.06, 0.81 }),
                new ManualSentimentAdjustmentRatingOffsetAdjuster(0.73),
                new RoundingManualSentimentAdjustmentAdjuster(5.0),
                new PercentManualSentimentAdjustmentAdjuster(-8.7),
                new CapManualSentimentAdjustmentAdjuster(44705.0),
            },
            new List<string>(),
            1.1,
            90927.4);
    }

    public double Evaluate(ManualSentimentAdjustment item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualSentimentAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
