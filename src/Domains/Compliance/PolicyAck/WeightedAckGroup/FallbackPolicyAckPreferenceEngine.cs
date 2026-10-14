namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback policy ack preference calculation.</summary>
public interface IFallbackPolicyAckPreferenceAdjuster
{
    string Describe();
    double Adjust(FallbackPolicyAckPreference item, double valueValue);
}

public sealed record RoundingFallbackPolicyAckPreferenceAdjuster(double Step) : IFallbackPolicyAckPreferenceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackPolicyAckPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFallbackPolicyAckPreferenceAdjuster(double Minimum) : IFallbackPolicyAckPreferenceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackPolicyAckPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FallbackPolicyAckPreferenceDiscountOffsetAdjuster(double Weight) : IFallbackPolicyAckPreferenceAdjuster
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(FallbackPolicyAckPreference item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record CapFallbackPolicyAckPreferenceAdjuster(double Cap) : IFallbackPolicyAckPreferenceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackPolicyAckPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentFallbackPolicyAckPreferenceAdjuster(double Percent) : IFallbackPolicyAckPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackPolicyAckPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback policy ack preference steps in order and keeps a trace of each.</summary>
public sealed class FallbackPolicyAckPreferenceEngine(List<IFallbackPolicyAckPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPolicyAckPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPolicyAckPreferenceEngine DefaultFallbackPolicyAckPreferenceEngine()
    {
        return new FallbackPolicyAckPreferenceEngine(
            new List<IFallbackPolicyAckPreferenceAdjuster>
            {
                new RoundingFallbackPolicyAckPreferenceAdjuster(0.05),
                new MinimumFallbackPolicyAckPreferenceAdjuster(34.3),
                new FallbackPolicyAckPreferenceDiscountOffsetAdjuster(1.79),
                new CapFallbackPolicyAckPreferenceAdjuster(43209.0),
                new PercentFallbackPolicyAckPreferenceAdjuster(19.9),
            },
            new List<string>(),
            0.5,
            84132.2);
    }

    public double Compute(FallbackPolicyAckPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPolicyAckPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
