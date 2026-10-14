namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment proposal calculation.</summary>
public interface IAssignmentProposalAdjuster
{
    string Describe();
    double Adjust(AssignmentProposal item, double valueValue);
}

public sealed record RoundingAssignmentProposalAdjuster(double Step) : IAssignmentProposalAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleAssignmentProposalAdjuster(Dictionary<string, double> Factors) : IAssignmentProposalAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AssignmentProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AssignmentProposalAmountOffsetAdjuster(double Weight) : IAssignmentProposalAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(AssignmentProposal item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record FlaggedAssignmentProposalAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAssignmentProposalAdjuster(double Percent) : IAssignmentProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the assignment proposal steps in order and keeps a trace of each.</summary>
public sealed class AssignmentProposalEngine(List<IAssignmentProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentProposalEngine DefaultAssignmentProposalEngine(Registry flags)
    {
        return new AssignmentProposalEngine(
            new List<IAssignmentProposalAdjuster>
            {
                new RoundingAssignmentProposalAdjuster(1.0),
                new TitleAssignmentProposalAdjuster(new Dictionary<string, double> { ["east"] = 0.79 }),
                new AssignmentProposalAmountOffsetAdjuster(1.27),
                new FlaggedAssignmentProposalAdjuster(flags, "assignment-proposal-global", -3.9),
                new PercentAssignmentProposalAdjuster(3.3),
            },
            new List<string>(),
            4.7,
            92951.8);
    }

    public double Evaluate(AssignmentProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
