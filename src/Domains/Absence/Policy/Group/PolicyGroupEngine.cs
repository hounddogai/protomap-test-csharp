namespace Acme.Workforce.Absence.Policy.Group;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the policy group calculation.</summary>
public interface IPolicyGroupAdjuster
{
    string Describe();
    double Adjust(PolicyGroup item, double valueValue);
}

public sealed record MinimumPolicyGroupAdjuster(double Minimum) : IPolicyGroupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPolicyGroupAdjuster(double Cap) : IPolicyGroupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPolicyGroupAdjuster(Registry Flags, string Flag, double Percent) : IPolicyGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPolicyGroupAdjuster(List<double> Bounds, List<double> Rates) : IPolicyGroupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyGroup item, double valueValue)
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

/// <summary>Runs the policy group steps in order and keeps a trace of each.</summary>
public sealed class PolicyGroupEngine(List<IPolicyGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyGroupEngine DefaultPolicyGroupEngine(Registry flags)
    {
        return new PolicyGroupEngine(
            new List<IPolicyGroupAdjuster>
            {
                new MinimumPolicyGroupAdjuster(45.5),
                new CapPolicyGroupAdjuster(2944.0),
                new FlaggedPolicyGroupAdjuster(flags, "policy-group-default", -7.2),
                new TieredPolicyGroupAdjuster(new List<double> { 2864.0, 8846.0 }, new List<double> { 1.13, 0.81 }),
            },
            new List<string>(),
            1.8,
            57856.3);
    }

    public double Run(PolicyGroup item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
