namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered sentiment target calculation.</summary>
public interface ITieredSentimentTargetStep
{
    string Describe();
    double Adjust(TieredSentimentTarget item, double valueValue);
}

public sealed record LabelTieredSentimentTargetAdjuster(Dictionary<string, double> Factors) : ITieredSentimentTargetStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TieredSentimentTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapTieredSentimentTargetAdjuster(double Cap) : ITieredSentimentTargetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredSentimentTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedTieredSentimentTargetAdjuster(Registry Flags, string Flag, double Percent) : ITieredSentimentTargetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredSentimentTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTieredSentimentTargetAdjuster(double Minimum) : ITieredSentimentTargetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredSentimentTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSentimentTargetExpiresAtOffsetAdjuster(double Weight) : ITieredSentimentTargetStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(TieredSentimentTarget item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record TieredTieredSentimentTargetAdjuster(List<double> Bounds, List<double> Rates) : ITieredSentimentTargetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredSentimentTarget item, double valueValue)
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

/// <summary>Runs the tiered sentiment target steps in order and keeps a trace of each.</summary>
public sealed class TieredSentimentTargetEngine(List<ITieredSentimentTargetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredSentimentTargetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredSentimentTargetEngine DefaultTieredSentimentTargetEngine(Registry flags)
    {
        return new TieredSentimentTargetEngine(
            new List<ITieredSentimentTargetStep>
            {
                new LabelTieredSentimentTargetAdjuster(new Dictionary<string, double> { ["online"] = 0.76 }),
                new CapTieredSentimentTargetAdjuster(36413.0),
                new FlaggedTieredSentimentTargetAdjuster(flags, "tiered-sentiment-target-capped", -7.0),
                new MinimumTieredSentimentTargetAdjuster(15.2),
                new TieredSentimentTargetExpiresAtOffsetAdjuster(0.14),
                new TieredTieredSentimentTargetAdjuster(
                    new List<double> { 3073.0, 4554.0, 8242.0 },
                    new List<double> { 0.95, 1.06, 0.91 }),
            },
            new List<string>(),
            2.8,
            80028.2);
    }

    public double Run(TieredSentimentTarget item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TieredSentimentTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
