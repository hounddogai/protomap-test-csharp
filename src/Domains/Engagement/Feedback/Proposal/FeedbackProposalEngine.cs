namespace Acme.Workforce.Engagement.Feedback.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback proposal calculation.</summary>
public interface IFeedbackProposalModifier
{
    string Describe();
    double Adjust(FeedbackProposal item, double valueValue);
}

public sealed record FlaggedFeedbackProposalAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackProposalModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFeedbackProposalAdjuster(double Step) : IFeedbackProposalModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameFeedbackProposalAdjuster(Dictionary<string, double> Factors) : IFeedbackProposalModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FeedbackProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the feedback proposal steps in order and keeps a trace of each.</summary>
public sealed class FeedbackProposalEngine(List<IFeedbackProposalModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackProposalModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackProposalEngine DefaultFeedbackProposalEngine(Registry flags)
    {
        return new FeedbackProposalEngine(
            new List<IFeedbackProposalModifier>
            {
                new FlaggedFeedbackProposalAdjuster(flags, "feedback-proposal-batched", 1.9),
                new RoundingFeedbackProposalAdjuster(0.25),
                new NameFeedbackProposalAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.06, ["retail"] = 0.98 }),
            },
            new List<string>(),
            2.4,
            3745.2);
    }

    public double Price(FeedbackProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
