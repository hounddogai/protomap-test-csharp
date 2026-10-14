namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>One step of the legacy policy ack group calculation.</summary>
public interface ILegacyPolicyAckGroupModifier
{
    string Describe();
    double Adjust(LegacyPolicyAckGroup item, double valueValue);
}

public sealed record LegacyPolicyAckGroupUpdatedAtOffsetAdjuster(double Weight) : ILegacyPolicyAckGroupModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(LegacyPolicyAckGroup item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record CapLegacyPolicyAckGroupAdjuster(double Cap) : ILegacyPolicyAckGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyPolicyAckGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelLegacyPolicyAckGroupAdjuster(Dictionary<string, double> Factors) : ILegacyPolicyAckGroupModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LegacyPolicyAckGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLegacyPolicyAckGroupAdjuster(double Percent) : ILegacyPolicyAckGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyPolicyAckGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy policy ack group steps in order and keeps a trace of each.</summary>
public sealed class LegacyPolicyAckGroupEngine(List<ILegacyPolicyAckGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPolicyAckGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPolicyAckGroupEngine DefaultLegacyPolicyAckGroupEngine()
    {
        return new LegacyPolicyAckGroupEngine(
            new List<ILegacyPolicyAckGroupModifier>
            {
                new LegacyPolicyAckGroupUpdatedAtOffsetAdjuster(0.98),
                new CapLegacyPolicyAckGroupAdjuster(48657.0),
                new ChannelLegacyPolicyAckGroupAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.03, ["retail"] = 0.95, ["south"] = 1.16 }),
                new PercentLegacyPolicyAckGroupAdjuster(-1.4),
            },
            new List<string>(),
            4.5,
            76680.0);
    }

    public double Compute(LegacyPolicyAckGroup item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPolicyAckGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
