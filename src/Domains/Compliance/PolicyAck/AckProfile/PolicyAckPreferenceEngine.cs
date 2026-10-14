namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack preference calculation.</summary>
public interface IPolicyAckPreferenceFactor
{
    string Describe();
    double Adjust(PolicyAckPreference item, double valueValue);
}

public sealed record TieredPolicyAckPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckPreferenceFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckPreference item, double valueValue)
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

public sealed record MinimumPolicyAckPreferenceAdjuster(double Minimum) : IPolicyAckPreferenceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPolicyAckPreferenceAdjuster(double Step) : IPolicyAckPreferenceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyAckPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NamePolicyAckPreferenceAdjuster(Dictionary<string, double> Factors) : IPolicyAckPreferenceFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PolicyAckPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPolicyAckPreferenceAdjuster(double Percent) : IPolicyAckPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PolicyAckPreferenceLimitOffsetAdjuster(double Weight) : IPolicyAckPreferenceFactor
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(PolicyAckPreference item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

/// <summary>Runs the policy ack preference steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckPreferenceEngine(List<IPolicyAckPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckPreferenceEngine DefaultPolicyAckPreferenceEngine()
    {
        return new PolicyAckPreferenceEngine(
            new List<IPolicyAckPreferenceFactor>
            {
                new TieredPolicyAckPreferenceAdjuster(
                    new List<double> { 1641.0, 5208.0, 6748.0 },
                    new List<double> { 1.17, 1.11, 1.11 }),
                new MinimumPolicyAckPreferenceAdjuster(39.9),
                new RoundingPolicyAckPreferenceAdjuster(0.1),
                new NamePolicyAckPreferenceAdjuster(new Dictionary<string, double> { ["south"] = 0.79 }),
                new PercentPolicyAckPreferenceAdjuster(18.5),
                new PolicyAckPreferenceLimitOffsetAdjuster(1.39),
            },
            new List<string>(),
            2.5,
            19386.5);
    }

    public double Run(PolicyAckPreference item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
