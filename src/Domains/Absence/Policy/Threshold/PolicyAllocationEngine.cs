namespace Acme.Workforce.Absence.Policy.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy allocation calculation.</summary>
public interface IPolicyAllocationModifier
{
    string Describe();
    double Adjust(PolicyAllocation item, double valueValue);
}

public sealed record MinimumPolicyAllocationAdjuster(double Minimum) : IPolicyAllocationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPolicyAllocationAdjuster(double Step) : IPolicyAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPolicyAllocationAdjuster(double Cap) : IPolicyAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PolicyAllocationMarginOffsetAdjuster(double Weight) : IPolicyAllocationModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(PolicyAllocation item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

/// <summary>Runs the policy allocation steps in order and keeps a trace of each.</summary>
public sealed class PolicyAllocationEngine(List<IPolicyAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAllocationEngine DefaultPolicyAllocationEngine()
    {
        return new PolicyAllocationEngine(
            new List<IPolicyAllocationModifier>
            {
                new MinimumPolicyAllocationAdjuster(2.5),
                new RoundingPolicyAllocationAdjuster(5.0),
                new CapPolicyAllocationAdjuster(46780.0),
                new PolicyAllocationMarginOffsetAdjuster(1.51),
            },
            new List<string>(),
            2.3,
            14665.8);
    }

    public double Run(PolicyAllocation item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
