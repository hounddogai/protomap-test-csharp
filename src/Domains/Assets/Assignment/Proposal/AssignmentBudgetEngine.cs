namespace Acme.Workforce.Assets.Assignment.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment budget calculation.</summary>
public interface IAssignmentBudgetStep
{
    string Describe();
    double Adjust(AssignmentBudget item, double valueValue);
}

public sealed record FlaggedAssignmentBudgetAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentBudgetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAssignmentBudgetAdjuster(double Cap) : IAssignmentBudgetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAssignmentBudgetAdjuster(double Minimum) : IAssignmentBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the assignment budget steps in order and keeps a trace of each.</summary>
public sealed class AssignmentBudgetEngine(List<IAssignmentBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentBudgetEngine DefaultAssignmentBudgetEngine(Registry flags)
    {
        return new AssignmentBudgetEngine(
            new List<IAssignmentBudgetStep>
            {
                new FlaggedAssignmentBudgetAdjuster(flags, "assignment-budget-adaptive", 7.9),
                new CapAssignmentBudgetAdjuster(44944.0),
                new MinimumAssignmentBudgetAdjuster(21.1),
            },
            new List<string>(),
            2.3,
            83836.0);
    }

    public double Run(AssignmentBudget item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
