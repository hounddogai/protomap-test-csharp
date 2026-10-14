namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack template calculation.</summary>
public interface IPolicyAckTemplateModifier
{
    string Describe();
    double Adjust(PolicyAckTemplate item, double valueValue);
}

public sealed record RoundingPolicyAckTemplateAdjuster(double Step) : IPolicyAckTemplateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyAckTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPolicyAckTemplateAdjuster(double Percent) : IPolicyAckTemplateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NamePolicyAckTemplateAdjuster(Dictionary<string, double> Factors) : IPolicyAckTemplateModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PolicyAckTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPolicyAckTemplateAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckTemplateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckTemplate item, double valueValue)
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

public sealed record MinimumPolicyAckTemplateAdjuster(double Minimum) : IPolicyAckTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPolicyAckTemplateAdjuster(double Cap) : IPolicyAckTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the policy ack template steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckTemplateEngine(List<IPolicyAckTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckTemplateEngine DefaultPolicyAckTemplateEngine()
    {
        return new PolicyAckTemplateEngine(
            new List<IPolicyAckTemplateModifier>
            {
                new RoundingPolicyAckTemplateAdjuster(0.25),
                new PercentPolicyAckTemplateAdjuster(3.8),
                new NamePolicyAckTemplateAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.34, ["retail"] = 1.35, ["north"] = 0.87 }),
                new TieredPolicyAckTemplateAdjuster(
                    new List<double> { 4396.0, 5460.0, 7389.0 },
                    new List<double> { 1.11, 0.85, 1.09 }),
                new MinimumPolicyAckTemplateAdjuster(49.4),
                new CapPolicyAckTemplateAdjuster(769.0),
            },
            new List<string>(),
            0.5,
            54848.5);
    }

    public double Run(PolicyAckTemplate item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
