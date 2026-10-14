namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment proposal calculation.</summary>
public interface ISentimentProposalAdjuster
{
    string Describe();
    double Adjust(SentimentProposal item, double valueValue);
}

public sealed record MinimumSentimentProposalAdjuster(double Minimum) : ISentimentProposalAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSentimentProposalAdjuster(List<double> Bounds, List<double> Rates) : ISentimentProposalAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentProposal item, double valueValue)
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

public sealed record FlaggedSentimentProposalAdjuster(Registry Flags, string Flag, double Percent) : ISentimentProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the sentiment proposal steps in order and keeps a trace of each.</summary>
public sealed class SentimentProposalEngine(List<ISentimentProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentProposalEngine DefaultSentimentProposalEngine(Registry flags)
    {
        return new SentimentProposalEngine(
            new List<ISentimentProposalAdjuster>
            {
                new MinimumSentimentProposalAdjuster(27.4),
                new TieredSentimentProposalAdjuster(
                    new List<double> { 7027.0, 7427.0 },
                    new List<double> { 0.83, 1.11 }),
                new FlaggedSentimentProposalAdjuster(flags, "sentiment-proposal-rolling", -5.9),
            },
            new List<string>(),
            2.8,
            60930.0);
    }

    public double Run(SentimentProposal item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
