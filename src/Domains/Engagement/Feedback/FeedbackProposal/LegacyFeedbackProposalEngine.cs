namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy feedback proposal calculation.</summary>
public interface ILegacyFeedbackProposalFactor
{
    string Describe();
    double Adjust(LegacyFeedbackProposal item, double valueValue);
}

public sealed record LabelLegacyFeedbackProposalAdjuster(Dictionary<string, double> Factors) : ILegacyFeedbackProposalFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LegacyFeedbackProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLegacyFeedbackProposalAdjuster(List<double> Bounds, List<double> Rates) : ILegacyFeedbackProposalFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyFeedbackProposal item, double valueValue)
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

public sealed record FlaggedLegacyFeedbackProposalAdjuster(Registry Flags, string Flag, double Percent) : ILegacyFeedbackProposalFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyFeedbackProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy feedback proposal steps in order and keeps a trace of each.</summary>
public sealed class LegacyFeedbackProposalEngine(List<ILegacyFeedbackProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyFeedbackProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyFeedbackProposalEngine DefaultLegacyFeedbackProposalEngine(Registry flags)
    {
        return new LegacyFeedbackProposalEngine(
            new List<ILegacyFeedbackProposalFactor>
            {
                new LabelLegacyFeedbackProposalAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.16, ["central"] = 0.7, ["retail"] = 0.97 }),
                new TieredLegacyFeedbackProposalAdjuster(
                    new List<double> { 4359.0, 8270.0 },
                    new List<double> { 1.04, 0.88 }),
                new FlaggedLegacyFeedbackProposalAdjuster(flags, "legacy-feedback-proposal-tiered", 1.0),
            },
            new List<string>(),
            2.9,
            69995.6);
    }

    public double Compute(LegacyFeedbackProposal item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyFeedbackProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
