namespace Acme.Workforce.Assets.Assignment.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment milestone calculation.</summary>
public interface IAssignmentMilestoneModifier
{
    string Describe();
    double Adjust(AssignmentMilestone item, double valueValue);
}

public sealed record FlaggedAssignmentMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AssignmentMilestoneSequenceOffsetAdjuster(double Weight) : IAssignmentMilestoneModifier
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(AssignmentMilestone item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record PercentAssignmentMilestoneAdjuster(double Percent) : IAssignmentMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAssignmentMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentMilestoneModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentMilestone item, double valueValue)
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

/// <summary>Runs the assignment milestone steps in order and keeps a trace of each.</summary>
public sealed class AssignmentMilestoneEngine(List<IAssignmentMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentMilestoneEngine DefaultAssignmentMilestoneEngine(Registry flags)
    {
        return new AssignmentMilestoneEngine(
            new List<IAssignmentMilestoneModifier>
            {
                new FlaggedAssignmentMilestoneAdjuster(flags, "assignment-milestone-primary", 6.2),
                new AssignmentMilestoneSequenceOffsetAdjuster(0.97),
                new PercentAssignmentMilestoneAdjuster(9.0),
                new TieredAssignmentMilestoneAdjuster(
                    new List<double> { 4446.0, 5318.0, 7350.0, 9387.0 },
                    new List<double> { 0.84, 0.87, 1.11, 1.16 }),
            },
            new List<string>(),
            0.4,
            68376.2);
    }

    public double Run(AssignmentMilestone item)
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
