namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict rate proposal calculation.</summary>
public interface IStrictRateProposalAdjuster
{
    string Describe();
    double Adjust(StrictRateProposal item, double valueValue);
}

public sealed record FlaggedStrictRateProposalAdjuster(Registry Flags, string Flag, double Percent) : IStrictRateProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictRateProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStrictRateProposalAdjuster(double Step) : IStrictRateProposalAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictRateProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SourceStrictRateProposalAdjuster(Dictionary<string, double> Factors) : IStrictRateProposalAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(StrictRateProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentStrictRateProposalAdjuster(double Percent) : IStrictRateProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictRateProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict rate proposal steps in order and keeps a trace of each.</summary>
public sealed class StrictRateProposalEngine(List<IStrictRateProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictRateProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictRateProposalEngine DefaultStrictRateProposalEngine(Registry flags)
    {
        return new StrictRateProposalEngine(
            new List<IStrictRateProposalAdjuster>
            {
                new FlaggedStrictRateProposalAdjuster(flags, "strict-rate-proposal-rolling", 4.6),
                new RoundingStrictRateProposalAdjuster(0.5),
                new SourceStrictRateProposalAdjuster(new Dictionary<string, double> { ["retail"] = 0.77 }),
                new PercentStrictRateProposalAdjuster(15.8),
            },
            new List<string>(),
            0.6,
            70236.6);
    }

    public double Compute(StrictRateProposal item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictRateProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
