namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment group calculation.</summary>
public interface ISentimentGroupAdjuster
{
    string Describe();
    double Adjust(SentimentGroup item, double valueValue);
}

public sealed record TieredSentimentGroupAdjuster(List<double> Bounds, List<double> Rates) : ISentimentGroupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentGroup item, double valueValue)
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

public sealed record RoundingSentimentGroupAdjuster(double Step) : ISentimentGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSentimentGroupAdjuster(Registry Flags, string Flag, double Percent) : ISentimentGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSentimentGroupAdjuster(double Cap) : ISentimentGroupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentSentimentGroupAdjuster(double Percent) : ISentimentGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSentimentGroupAdjuster(double Minimum) : ISentimentGroupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the sentiment group steps in order and keeps a trace of each.</summary>
public sealed class SentimentGroupEngine(List<ISentimentGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentGroupEngine DefaultSentimentGroupEngine(Registry flags)
    {
        return new SentimentGroupEngine(
            new List<ISentimentGroupAdjuster>
            {
                new TieredSentimentGroupAdjuster(new List<double> { 66.0, 3866.0 }, new List<double> { 0.88, 1.04 }),
                new RoundingSentimentGroupAdjuster(0.5),
                new FlaggedSentimentGroupAdjuster(flags, "sentiment-group-regional", -8.0),
                new CapSentimentGroupAdjuster(20095.0),
                new PercentSentimentGroupAdjuster(-5.5),
                new MinimumSentimentGroupAdjuster(22.0),
            },
            new List<string>(),
            0.7,
            54682.1);
    }

    public double Evaluate(SentimentGroup item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
