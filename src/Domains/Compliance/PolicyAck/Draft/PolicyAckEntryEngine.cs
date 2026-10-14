namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack entry calculation.</summary>
public interface IPolicyAckEntryAdjuster
{
    string Describe();
    double Adjust(PolicyAckEntry item, double valueValue);
}

public sealed record RoundingPolicyAckEntryAdjuster(double Step) : IPolicyAckEntryAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyAckEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PolicyAckEntryAttemptsOffsetAdjuster(double Weight) : IPolicyAckEntryAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(PolicyAckEntry item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record MinimumPolicyAckEntryAdjuster(double Minimum) : IPolicyAckEntryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPolicyAckEntryAdjuster(double Percent) : IPolicyAckEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy ack entry steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckEntryEngine(List<IPolicyAckEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckEntryEngine DefaultPolicyAckEntryEngine()
    {
        return new PolicyAckEntryEngine(
            new List<IPolicyAckEntryAdjuster>
            {
                new RoundingPolicyAckEntryAdjuster(1.0),
                new PolicyAckEntryAttemptsOffsetAdjuster(1.63),
                new MinimumPolicyAckEntryAdjuster(39.4),
                new PercentPolicyAckEntryAdjuster(12.0),
            },
            new List<string>(),
            2.5,
            41792.1);
    }

    public double Evaluate(PolicyAckEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
