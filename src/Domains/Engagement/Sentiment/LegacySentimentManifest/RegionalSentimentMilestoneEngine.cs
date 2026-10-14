namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional sentiment milestone calculation.</summary>
public interface IRegionalSentimentMilestoneAdjuster
{
    string Describe();
    double Adjust(RegionalSentimentMilestone item, double valueValue);
}

public sealed record PercentRegionalSentimentMilestoneAdjuster(double Percent) : IRegionalSentimentMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalSentimentMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalSentimentMilestoneAdjuster(double Step) : IRegionalSentimentMilestoneAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalSentimentMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionalSentimentMilestonePriorityOffsetAdjuster(double Weight) : IRegionalSentimentMilestoneAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(RegionalSentimentMilestone item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record LabelRegionalSentimentMilestoneAdjuster(Dictionary<string, double> Factors) : IRegionalSentimentMilestoneAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RegionalSentimentMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRegionalSentimentMilestoneAdjuster(double Minimum) : IRegionalSentimentMilestoneAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalSentimentMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the regional sentiment milestone steps in order and keeps a trace of each.</summary>
public sealed class RegionalSentimentMilestoneEngine(List<IRegionalSentimentMilestoneAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSentimentMilestoneAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSentimentMilestoneEngine DefaultRegionalSentimentMilestoneEngine()
    {
        return new RegionalSentimentMilestoneEngine(
            new List<IRegionalSentimentMilestoneAdjuster>
            {
                new PercentRegionalSentimentMilestoneAdjuster(21.3),
                new RoundingRegionalSentimentMilestoneAdjuster(0.25),
                new RegionalSentimentMilestonePriorityOffsetAdjuster(0.65),
                new LabelRegionalSentimentMilestoneAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.98, ["north"] = 0.75, ["central"] = 0.81 }),
                new MinimumRegionalSentimentMilestoneAdjuster(31.3),
            },
            new List<string>(),
            0.3,
            86169.7);
    }

    public double Compute(RegionalSentimentMilestone item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSentimentMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
