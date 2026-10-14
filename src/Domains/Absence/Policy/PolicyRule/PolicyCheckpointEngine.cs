namespace Acme.Workforce.Absence.Policy.PolicyRule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the policy checkpoint calculation.</summary>
public interface IPolicyCheckpointStep
{
    string Describe();
    double Adjust(PolicyCheckpoint item, double valueValue);
}

public sealed record DepartmentPolicyCheckpointAdjuster(Dictionary<string, double> Factors) : IPolicyCheckpointStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PolicyCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPolicyCheckpointAdjuster(double Cap) : IPolicyCheckpointStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPolicyCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IPolicyCheckpointStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPolicyCheckpointAdjuster(double Percent) : IPolicyCheckpointStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy checkpoint steps in order and keeps a trace of each.</summary>
public sealed class PolicyCheckpointEngine(List<IPolicyCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyCheckpointEngine DefaultPolicyCheckpointEngine(Registry flags)
    {
        return new PolicyCheckpointEngine(
            new List<IPolicyCheckpointStep>
            {
                new DepartmentPolicyCheckpointAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.22, ["north"] = 1.14, ["central"] = 0.78 }),
                new CapPolicyCheckpointAdjuster(1198.0),
                new FlaggedPolicyCheckpointAdjuster(flags, "policy-checkpoint-global", -8.0),
                new PercentPolicyCheckpointAdjuster(-20.6),
            },
            new List<string>(),
            3.0,
            13688.9);
    }

    public double Run(PolicyCheckpoint item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
