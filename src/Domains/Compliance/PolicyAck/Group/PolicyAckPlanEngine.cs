namespace Acme.Workforce.Compliance.PolicyAck.Group;

/// <summary>One step of the policy ack plan calculation.</summary>
public interface IPolicyAckPlanModifier
{
    string Describe();
    double Adjust(PolicyAckPlan item, double valueValue);
}

public sealed record MinimumPolicyAckPlanAdjuster(double Minimum) : IPolicyAckPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PolicyAckPlanBudgetOffsetAdjuster(double Weight) : IPolicyAckPlanModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(PolicyAckPlan item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record TieredPolicyAckPlanAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckPlan item, double valueValue)
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

public sealed record CapPolicyAckPlanAdjuster(double Cap) : IPolicyAckPlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the policy ack plan steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckPlanEngine(List<IPolicyAckPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckPlanEngine DefaultPolicyAckPlanEngine()
    {
        return new PolicyAckPlanEngine(
            new List<IPolicyAckPlanModifier>
            {
                new MinimumPolicyAckPlanAdjuster(32.6),
                new PolicyAckPlanBudgetOffsetAdjuster(0.5),
                new TieredPolicyAckPlanAdjuster(
                    new List<double> { 5724.0, 7518.0, 8011.0, 8041.0 },
                    new List<double> { 0.97, 0.92, 1.06, 1.16 }),
                new CapPolicyAckPlanAdjuster(48787.0),
            },
            new List<string>(),
            0.4,
            3037.6);
    }

    public double Run(PolicyAckPlan item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
