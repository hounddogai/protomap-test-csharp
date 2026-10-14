namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy assignment calculation.</summary>
public interface IPolicyAssignmentModifier
{
    string Describe();
    double Adjust(PolicyAssignment item, double valueValue);
}

public sealed record CapPolicyAssignmentAdjuster(double Cap) : IPolicyAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPolicyAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAssignmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAssignment item, double valueValue)
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

public sealed record MinimumPolicyAssignmentAdjuster(double Minimum) : IPolicyAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PolicyAssignmentLeaveDaysOffsetAdjuster(double Weight) : IPolicyAssignmentModifier
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(PolicyAssignment item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record FlaggedPolicyAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IPolicyAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy assignment steps in order and keeps a trace of each.</summary>
public sealed class PolicyAssignmentEngine(List<IPolicyAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAssignmentEngine DefaultPolicyAssignmentEngine(Registry flags)
    {
        return new PolicyAssignmentEngine(
            new List<IPolicyAssignmentModifier>
            {
                new CapPolicyAssignmentAdjuster(17249.0),
                new TieredPolicyAssignmentAdjuster(
                    new List<double> { 2215.0, 3677.0, 4702.0 },
                    new List<double> { 1.1, 1.16, 0.85 }),
                new MinimumPolicyAssignmentAdjuster(21.2),
                new PolicyAssignmentLeaveDaysOffsetAdjuster(1.63),
                new FlaggedPolicyAssignmentAdjuster(flags, "policy-assignment-legacy", 4.5),
            },
            new List<string>(),
            4.7,
            15870.9);
    }

    public double Run(PolicyAssignment item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
