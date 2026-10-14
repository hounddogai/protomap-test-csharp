namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the leave request proposal calculation.</summary>
public interface ILeaveRequestProposalModifier
{
    string Describe();
    double Adjust(LeaveRequestProposal item, double valueValue);
}

public sealed record CapLeaveRequestProposalAdjuster(double Cap) : ILeaveRequestProposalModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLeaveRequestProposalAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestProposalModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLeaveRequestProposalAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestProposalModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestProposal item, double valueValue)
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

public sealed record LeaveRequestProposalExpiresAtOffsetAdjuster(double Weight) : ILeaveRequestProposalModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(LeaveRequestProposal item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the leave request proposal steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestProposalEngine(List<ILeaveRequestProposalModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestProposalModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestProposalEngine DefaultLeaveRequestProposalEngine(Registry flags)
    {
        return new LeaveRequestProposalEngine(
            new List<ILeaveRequestProposalModifier>
            {
                new CapLeaveRequestProposalAdjuster(17501.0),
                new FlaggedLeaveRequestProposalAdjuster(flags, "leave-request-proposal-manual", -2.1),
                new TieredLeaveRequestProposalAdjuster(
                    new List<double> { 663.0, 2467.0, 9545.0 },
                    new List<double> { 1.07, 0.8, 0.84 }),
                new LeaveRequestProposalExpiresAtOffsetAdjuster(1.33),
            },
            new List<string>(),
            3.7,
            24489.0);
    }

    public double Run(LeaveRequestProposal item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
