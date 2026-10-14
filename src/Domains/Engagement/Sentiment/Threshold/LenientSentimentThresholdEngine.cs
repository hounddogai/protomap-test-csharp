namespace Acme.Workforce.Engagement.Sentiment.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient sentiment threshold calculation.</summary>
public interface ILenientSentimentThresholdFactor
{
    string Describe();
    double Adjust(LenientSentimentThreshold item, double valueValue);
}

public sealed record CountryLenientSentimentThresholdAdjuster(Dictionary<string, double> Factors) : ILenientSentimentThresholdFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(LenientSentimentThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLenientSentimentThresholdAdjuster(List<double> Bounds, List<double> Rates) : ILenientSentimentThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientSentimentThreshold item, double valueValue)
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

public sealed record RoundingLenientSentimentThresholdAdjuster(double Step) : ILenientSentimentThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientSentimentThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLenientSentimentThresholdAdjuster(Registry Flags, string Flag, double Percent) : ILenientSentimentThresholdFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientSentimentThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientSentimentThresholdDurationMinutesOffsetAdjuster(double Weight) : ILenientSentimentThresholdFactor
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(LenientSentimentThreshold item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

/// <summary>Runs the lenient sentiment threshold steps in order and keeps a trace of each.</summary>
public sealed class LenientSentimentThresholdEngine(List<ILenientSentimentThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientSentimentThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientSentimentThresholdEngine DefaultLenientSentimentThresholdEngine(Registry flags)
    {
        return new LenientSentimentThresholdEngine(
            new List<ILenientSentimentThresholdFactor>
            {
                new CountryLenientSentimentThresholdAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.91, ["central"] = 1.14 }),
                new TieredLenientSentimentThresholdAdjuster(
                    new List<double> { 3064.0, 5215.0, 6471.0 },
                    new List<double> { 1.07, 1.16, 1.07 }),
                new RoundingLenientSentimentThresholdAdjuster(5.0),
                new FlaggedLenientSentimentThresholdAdjuster(flags, "lenient-sentiment-threshold-scheduled", -0.8),
                new LenientSentimentThresholdDurationMinutesOffsetAdjuster(0.9),
            },
            new List<string>(),
            1.1,
            4529.8);
    }

    public double Evaluate(LenientSentimentThreshold item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientSentimentThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
