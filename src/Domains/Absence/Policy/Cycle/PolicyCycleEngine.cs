namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the policy cycle calculation.</summary>
public interface IPolicyCycleFactor
{
    string Describe();
    double Adjust(PolicyCycle item, double valueValue);
}

public sealed record NamePolicyCycleAdjuster(Dictionary<string, double> Factors) : IPolicyCycleFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PolicyCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPolicyCycleAdjuster(Registry Flags, string Flag, double Percent) : IPolicyCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPolicyCycleAdjuster(double Minimum) : IPolicyCycleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the policy cycle steps in order and keeps a trace of each.</summary>
public sealed class PolicyCycleEngine(List<IPolicyCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyCycleEngine DefaultPolicyCycleEngine(Registry flags)
    {
        return new PolicyCycleEngine(
            new List<IPolicyCycleFactor>
            {
                new NamePolicyCycleAdjuster(new Dictionary<string, double> { ["retail"] = 0.79 }),
                new FlaggedPolicyCycleAdjuster(flags, "policy-cycle-rolling", 0.4),
                new MinimumPolicyCycleAdjuster(1.7),
            },
            new List<string>(),
            1.1,
            25106.2);
    }

    public double Evaluate(PolicyCycle item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
