namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling sentiment summary calculation.</summary>
public interface IRollingSentimentSummaryAdjuster
{
    string Describe();
    double Adjust(RollingSentimentSummary item, double valueValue);
}

public sealed record PercentRollingSentimentSummaryAdjuster(double Percent) : IRollingSentimentSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingSentimentSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRollingSentimentSummaryAdjuster(double Cap) : IRollingSentimentSummaryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingSentimentSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumRollingSentimentSummaryAdjuster(double Minimum) : IRollingSentimentSummaryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingSentimentSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RollingSentimentSummaryBatchSizeOffsetAdjuster(double Weight) : IRollingSentimentSummaryAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(RollingSentimentSummary item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record RoundingRollingSentimentSummaryAdjuster(double Step) : IRollingSentimentSummaryAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingSentimentSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SourceRollingSentimentSummaryAdjuster(Dictionary<string, double> Factors) : IRollingSentimentSummaryAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(RollingSentimentSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the rolling sentiment summary steps in order and keeps a trace of each.</summary>
public sealed class RollingSentimentSummaryEngine(List<IRollingSentimentSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingSentimentSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingSentimentSummaryEngine DefaultRollingSentimentSummaryEngine()
    {
        return new RollingSentimentSummaryEngine(
            new List<IRollingSentimentSummaryAdjuster>
            {
                new PercentRollingSentimentSummaryAdjuster(10.7),
                new CapRollingSentimentSummaryAdjuster(22370.0),
                new MinimumRollingSentimentSummaryAdjuster(47.9),
                new RollingSentimentSummaryBatchSizeOffsetAdjuster(1.7),
                new RoundingRollingSentimentSummaryAdjuster(0.25),
                new SourceRollingSentimentSummaryAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.94, ["south"] = 0.91 }),
            },
            new List<string>(),
            0.1,
            43681.9);
    }

    public double Price(RollingSentimentSummary item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RollingSentimentSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
