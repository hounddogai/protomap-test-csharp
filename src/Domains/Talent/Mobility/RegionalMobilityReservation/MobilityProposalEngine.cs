namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility proposal calculation.</summary>
public interface IMobilityProposalStep
{
    string Describe();
    double Adjust(MobilityProposal item, double valueValue);
}

public sealed record PercentMobilityProposalAdjuster(double Percent) : IMobilityProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MobilityProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingMobilityProposalAdjuster(double Step) : IMobilityProposalStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MobilityProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumMobilityProposalAdjuster(double Minimum) : IMobilityProposalStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the mobility proposal steps in order and keeps a trace of each.</summary>
public sealed class MobilityProposalEngine(List<IMobilityProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityProposalEngine DefaultMobilityProposalEngine()
    {
        return new MobilityProposalEngine(
            new List<IMobilityProposalStep>
            {
                new PercentMobilityProposalAdjuster(-24.5),
                new RoundingMobilityProposalAdjuster(5.0),
                new MinimumMobilityProposalAdjuster(24.2),
            },
            new List<string>(),
            0.5,
            33908.7);
    }

    public double Compute(MobilityProposal item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
