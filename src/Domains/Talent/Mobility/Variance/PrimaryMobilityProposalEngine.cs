namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary mobility proposal calculation.</summary>
public interface IPrimaryMobilityProposalStep
{
    string Describe();
    double Adjust(PrimaryMobilityProposal item, double valueValue);
}

public sealed record PercentPrimaryMobilityProposalAdjuster(double Percent) : IPrimaryMobilityProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryMobilityProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPrimaryMobilityProposalAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryMobilityProposalStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryMobilityProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryMobilityProposalAdjuster(double Step) : IPrimaryMobilityProposalStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryMobilityProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary mobility proposal steps in order and keeps a trace of each.</summary>
public sealed class PrimaryMobilityProposalEngine(List<IPrimaryMobilityProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryMobilityProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryMobilityProposalEngine DefaultPrimaryMobilityProposalEngine(Registry flags)
    {
        return new PrimaryMobilityProposalEngine(
            new List<IPrimaryMobilityProposalStep>
            {
                new PercentPrimaryMobilityProposalAdjuster(-3.6),
                new FlaggedPrimaryMobilityProposalAdjuster(flags, "primary-mobility-proposal-scheduled", 6.6),
                new RoundingPrimaryMobilityProposalAdjuster(0.5),
            },
            new List<string>(),
            1.1,
            2668.1);
    }

    public double Run(PrimaryMobilityProposal item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryMobilityProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
