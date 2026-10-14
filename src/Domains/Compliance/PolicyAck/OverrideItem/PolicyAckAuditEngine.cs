namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>One step of the policy ack audit calculation.</summary>
public interface IPolicyAckAuditFactor
{
    string Describe();
    double Adjust(PolicyAckAudit item, double valueValue);
}

public sealed record PercentPolicyAckAuditAdjuster(double Percent) : IPolicyAckAuditFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPolicyAckAuditAdjuster(double Cap) : IPolicyAckAuditFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPolicyAckAuditAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckAuditFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckAudit item, double valueValue)
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

/// <summary>Runs the policy ack audit steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckAuditEngine(List<IPolicyAckAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckAuditEngine DefaultPolicyAckAuditEngine()
    {
        return new PolicyAckAuditEngine(
            new List<IPolicyAckAuditFactor>
            {
                new PercentPolicyAckAuditAdjuster(18.5),
                new CapPolicyAckAuditAdjuster(23912.0),
                new TieredPolicyAckAuditAdjuster(
                    new List<double> { 104.0, 1802.0, 2698.0, 8179.0 },
                    new List<double> { 0.8, 1.09, 0.91, 0.82 }),
            },
            new List<string>(),
            1.5,
            79280.9);
    }

    public double Evaluate(PolicyAckAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
