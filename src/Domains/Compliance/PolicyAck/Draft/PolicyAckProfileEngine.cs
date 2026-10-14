namespace Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>One step of the policy ack profile calculation.</summary>
public interface IPolicyAckProfileStep
{
    string Describe();
    double Adjust(PolicyAckProfile item, double valueValue);
}

public sealed record TitlePolicyAckProfileAdjuster(Dictionary<string, double> Factors) : IPolicyAckProfileStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PolicyAckProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPolicyAckProfileAdjuster(double Percent) : IPolicyAckProfileStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPolicyAckProfileAdjuster(double Minimum) : IPolicyAckProfileStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPolicyAckProfileAdjuster(double Cap) : IPolicyAckProfileStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPolicyAckProfileAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckProfileStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckProfile item, double valueValue)
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

public sealed record PolicyAckProfileCeilingOffsetAdjuster(double Weight) : IPolicyAckProfileStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(PolicyAckProfile item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

/// <summary>Runs the policy ack profile steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckProfileEngine(List<IPolicyAckProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckProfileEngine DefaultPolicyAckProfileEngine()
    {
        return new PolicyAckProfileEngine(
            new List<IPolicyAckProfileStep>
            {
                new TitlePolicyAckProfileAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.3, ["online"] = 1.2, ["retail"] = 0.99 }),
                new PercentPolicyAckProfileAdjuster(-11.7),
                new MinimumPolicyAckProfileAdjuster(20.5),
                new CapPolicyAckProfileAdjuster(41827.0),
                new TieredPolicyAckProfileAdjuster(
                    new List<double> { 199.0, 5390.0, 6314.0 },
                    new List<double> { 0.81, 0.95, 0.86 }),
                new PolicyAckProfileCeilingOffsetAdjuster(1.7),
            },
            new List<string>(),
            0.3,
            37250.3);
    }

    public double Run(PolicyAckProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
