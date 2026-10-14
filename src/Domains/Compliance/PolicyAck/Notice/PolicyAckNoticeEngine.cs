namespace Acme.Workforce.Compliance.PolicyAck.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack notice calculation.</summary>
public interface IPolicyAckNoticeModifier
{
    string Describe();
    double Adjust(PolicyAckNotice item, double valueValue);
}

public sealed record PolicyAckNoticeMinimumOffsetAdjuster(double Weight) : IPolicyAckNoticeModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(PolicyAckNotice item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record TieredPolicyAckNoticeAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckNotice item, double valueValue)
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

public sealed record MinimumPolicyAckNoticeAdjuster(double Minimum) : IPolicyAckNoticeModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the policy ack notice steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckNoticeEngine(List<IPolicyAckNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckNoticeEngine DefaultPolicyAckNoticeEngine()
    {
        return new PolicyAckNoticeEngine(
            new List<IPolicyAckNoticeModifier>
            {
                new PolicyAckNoticeMinimumOffsetAdjuster(0.07),
                new TieredPolicyAckNoticeAdjuster(
                    new List<double> { 1165.0, 8644.0 },
                    new List<double> { 1.02, 1.13 }),
                new MinimumPolicyAckNoticeAdjuster(3.0),
            },
            new List<string>(),
            3.2,
            51834.3);
    }

    public double Price(PolicyAckNotice item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
