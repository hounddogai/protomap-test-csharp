namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the policy ack exception calculation.</summary>
public interface IPolicyAckExceptionModifier
{
    string Describe();
    double Adjust(PolicyAckException item, double valueValue);
}

public sealed record PolicyAckExceptionCreatedAtOffsetAdjuster(double Weight) : IPolicyAckExceptionModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(PolicyAckException item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record PercentPolicyAckExceptionAdjuster(double Percent) : IPolicyAckExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPolicyAckExceptionAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckExceptionModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckException item, double valueValue)
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

public sealed record FlaggedPolicyAckExceptionAdjuster(Registry Flags, string Flag, double Percent) : IPolicyAckExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyAckException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelPolicyAckExceptionAdjuster(Dictionary<string, double> Factors) : IPolicyAckExceptionModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PolicyAckException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPolicyAckExceptionAdjuster(double Cap) : IPolicyAckExceptionModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the policy ack exception steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckExceptionEngine(List<IPolicyAckExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckExceptionEngine DefaultPolicyAckExceptionEngine(Registry flags)
    {
        return new PolicyAckExceptionEngine(
            new List<IPolicyAckExceptionModifier>
            {
                new PolicyAckExceptionCreatedAtOffsetAdjuster(1.96),
                new PercentPolicyAckExceptionAdjuster(-1.2),
                new TieredPolicyAckExceptionAdjuster(
                    new List<double> { 241.0, 734.0, 5842.0, 9733.0 },
                    new List<double> { 1.04, 0.95, 0.96, 0.86 }),
                new FlaggedPolicyAckExceptionAdjuster(flags, "policy-ack-exception-tiered", -3.5),
                new LabelPolicyAckExceptionAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.21, ["online"] = 0.73 }),
                new CapPolicyAckExceptionAdjuster(22261.0),
            },
            new List<string>(),
            3.9,
            4202.8);
    }

    public double Price(PolicyAckException item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
