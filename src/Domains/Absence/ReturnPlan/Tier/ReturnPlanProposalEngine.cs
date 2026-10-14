namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan proposal calculation.</summary>
public interface IReturnPlanProposalAdjuster
{
    string Describe();
    double Adjust(ReturnPlanProposal item, double valueValue);
}

public sealed record RoundingReturnPlanProposalAdjuster(double Step) : IReturnPlanProposalAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ReturnPlanProposalExpiresAtOffsetAdjuster(double Weight) : IReturnPlanProposalAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(ReturnPlanProposal item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record CapReturnPlanProposalAdjuster(double Cap) : IReturnPlanProposalAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedReturnPlanProposalAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumReturnPlanProposalAdjuster(double Minimum) : IReturnPlanProposalAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the return plan proposal steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanProposalEngine(List<IReturnPlanProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanProposalEngine DefaultReturnPlanProposalEngine(Registry flags)
    {
        return new ReturnPlanProposalEngine(
            new List<IReturnPlanProposalAdjuster>
            {
                new RoundingReturnPlanProposalAdjuster(0.1),
                new ReturnPlanProposalExpiresAtOffsetAdjuster(1.24),
                new CapReturnPlanProposalAdjuster(25897.0),
                new FlaggedReturnPlanProposalAdjuster(flags, "return-plan-proposal-lenient", -6.5),
                new MinimumReturnPlanProposalAdjuster(43.3),
            },
            new List<string>(),
            3.1,
            24439.6);
    }

    public double Price(ReturnPlanProposal item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
