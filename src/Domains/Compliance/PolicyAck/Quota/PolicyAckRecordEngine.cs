namespace Acme.Workforce.Compliance.PolicyAck.Quota;

/// <summary>One step of the policy ack record calculation.</summary>
public interface IPolicyAckRecordAdjuster
{
    string Describe();
    double Adjust(PolicyAckRecord item, double valueValue);
}

public sealed record CapPolicyAckRecordAdjuster(double Cap) : IPolicyAckRecordAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SegmentPolicyAckRecordAdjuster(Dictionary<string, double> Factors) : IPolicyAckRecordAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PolicyAckRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPolicyAckRecordAdjuster(double Minimum) : IPolicyAckRecordAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PolicyAckRecordMinimumOffsetAdjuster(double Weight) : IPolicyAckRecordAdjuster
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(PolicyAckRecord item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the policy ack record steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckRecordEngine(List<IPolicyAckRecordAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckRecordAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckRecordEngine DefaultPolicyAckRecordEngine()
    {
        return new PolicyAckRecordEngine(
            new List<IPolicyAckRecordAdjuster>
            {
                new CapPolicyAckRecordAdjuster(26793.0),
                new SegmentPolicyAckRecordAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.07, ["retail"] = 0.93 }),
                new MinimumPolicyAckRecordAdjuster(37.1),
                new PolicyAckRecordMinimumOffsetAdjuster(0.9),
            },
            new List<string>(),
            1.3,
            32188.6);
    }

    public double Price(PolicyAckRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
