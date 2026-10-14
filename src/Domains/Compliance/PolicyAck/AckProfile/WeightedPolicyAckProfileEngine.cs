namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted policy ack profile calculation.</summary>
public interface IWeightedPolicyAckProfileModifier
{
    string Describe();
    double Adjust(WeightedPolicyAckProfile item, double valueValue);
}

public sealed record CapWeightedPolicyAckProfileAdjuster(double Cap) : IWeightedPolicyAckProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedPolicyAckProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumWeightedPolicyAckProfileAdjuster(double Minimum) : IWeightedPolicyAckProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedPolicyAckProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingWeightedPolicyAckProfileAdjuster(double Step) : IWeightedPolicyAckProfileModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedPolicyAckProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CountryWeightedPolicyAckProfileAdjuster(Dictionary<string, double> Factors) : IWeightedPolicyAckProfileModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(WeightedPolicyAckProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedWeightedPolicyAckProfileAdjuster(Registry Flags, string Flag, double Percent) : IWeightedPolicyAckProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedPolicyAckProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentWeightedPolicyAckProfileAdjuster(double Percent) : IWeightedPolicyAckProfileModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedPolicyAckProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted policy ack profile steps in order and keeps a trace of each.</summary>
public sealed class WeightedPolicyAckProfileEngine(List<IWeightedPolicyAckProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedPolicyAckProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedPolicyAckProfileEngine DefaultWeightedPolicyAckProfileEngine(Registry flags)
    {
        return new WeightedPolicyAckProfileEngine(
            new List<IWeightedPolicyAckProfileModifier>
            {
                new CapWeightedPolicyAckProfileAdjuster(48014.0),
                new MinimumWeightedPolicyAckProfileAdjuster(29.4),
                new RoundingWeightedPolicyAckProfileAdjuster(0.05),
                new CountryWeightedPolicyAckProfileAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.12, ["east"] = 1.29 }),
                new FlaggedWeightedPolicyAckProfileAdjuster(flags, "weighted-policy-ack-profile-tiered", -4.2),
                new PercentWeightedPolicyAckProfileAdjuster(-14.1),
            },
            new List<string>(),
            0.4,
            7348.6);
    }

    public double Price(WeightedPolicyAckProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedPolicyAckProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
