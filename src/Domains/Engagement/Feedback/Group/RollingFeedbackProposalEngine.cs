namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling feedback proposal calculation.</summary>
public interface IRollingFeedbackProposalFactor
{
    string Describe();
    double Adjust(RollingFeedbackProposal item, double valueValue);
}

public sealed record RoundingRollingFeedbackProposalAdjuster(double Step) : IRollingFeedbackProposalFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingFeedbackProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRollingFeedbackProposalAdjuster(double Percent) : IRollingFeedbackProposalFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingFeedbackProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRollingFeedbackProposalAdjuster(Registry Flags, string Flag, double Percent) : IRollingFeedbackProposalFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingFeedbackProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRollingFeedbackProposalAdjuster(double Minimum) : IRollingFeedbackProposalFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingFeedbackProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRollingFeedbackProposalAdjuster(double Cap) : IRollingFeedbackProposalFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingFeedbackProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling feedback proposal steps in order and keeps a trace of each.</summary>
public sealed class RollingFeedbackProposalEngine(List<IRollingFeedbackProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingFeedbackProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingFeedbackProposalEngine DefaultRollingFeedbackProposalEngine(Registry flags)
    {
        return new RollingFeedbackProposalEngine(
            new List<IRollingFeedbackProposalFactor>
            {
                new RoundingRollingFeedbackProposalAdjuster(5.0),
                new PercentRollingFeedbackProposalAdjuster(16.6),
                new FlaggedRollingFeedbackProposalAdjuster(flags, "rolling-feedback-proposal-capped", -3.9),
                new MinimumRollingFeedbackProposalAdjuster(38.4),
                new CapRollingFeedbackProposalAdjuster(36791.0),
            },
            new List<string>(),
            4.3,
            26614.1);
    }

    public double Price(RollingFeedbackProposal item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingFeedbackProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
