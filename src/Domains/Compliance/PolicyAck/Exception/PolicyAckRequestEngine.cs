namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the policy ack request calculation.</summary>
public interface IPolicyAckRequestModifier
{
    string Describe();
    double Adjust(PolicyAckRequest item, double valueValue);
}

public sealed record PolicyAckRequestTenureMonthsOffsetAdjuster(double Weight) : IPolicyAckRequestModifier
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(PolicyAckRequest item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record FlaggedPolicyAckRequestAdjuster(Registry Flags, string Flag, double Percent) : IPolicyAckRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyAckRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPolicyAckRequestAdjuster(double Percent) : IPolicyAckRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPolicyAckRequestAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckRequest item, double valueValue)
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

public sealed record SegmentPolicyAckRequestAdjuster(Dictionary<string, double> Factors) : IPolicyAckRequestModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PolicyAckRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPolicyAckRequestAdjuster(double Cap) : IPolicyAckRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the policy ack request steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckRequestEngine(List<IPolicyAckRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckRequestEngine DefaultPolicyAckRequestEngine(Registry flags)
    {
        return new PolicyAckRequestEngine(
            new List<IPolicyAckRequestModifier>
            {
                new PolicyAckRequestTenureMonthsOffsetAdjuster(0.1),
                new FlaggedPolicyAckRequestAdjuster(flags, "policy-ack-request-weighted", -9.6),
                new PercentPolicyAckRequestAdjuster(7.8),
                new TieredPolicyAckRequestAdjuster(
                    new List<double> { 2774.0, 4491.0 },
                    new List<double> { 0.87, 0.82 }),
                new SegmentPolicyAckRequestAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.3, ["north"] = 1.36 }),
                new CapPolicyAckRequestAdjuster(49655.0),
            },
            new List<string>(),
            1.4,
            18172.8);
    }

    public double Run(PolicyAckRequest item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
