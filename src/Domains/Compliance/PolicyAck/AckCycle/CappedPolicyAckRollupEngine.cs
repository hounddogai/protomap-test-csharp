namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped policy ack rollup calculation.</summary>
public interface ICappedPolicyAckRollupModifier
{
    string Describe();
    double Adjust(CappedPolicyAckRollup item, double valueValue);
}

public sealed record FlaggedCappedPolicyAckRollupAdjuster(Registry Flags, string Flag, double Percent) : ICappedPolicyAckRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedPolicyAckRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentCappedPolicyAckRollupAdjuster(Dictionary<string, double> Factors) : ICappedPolicyAckRollupModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(CappedPolicyAckRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCappedPolicyAckRollupAdjuster(double Cap) : ICappedPolicyAckRollupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedPolicyAckRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped policy ack rollup steps in order and keeps a trace of each.</summary>
public sealed class CappedPolicyAckRollupEngine(List<ICappedPolicyAckRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPolicyAckRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPolicyAckRollupEngine DefaultCappedPolicyAckRollupEngine(Registry flags)
    {
        return new CappedPolicyAckRollupEngine(
            new List<ICappedPolicyAckRollupModifier>
            {
                new FlaggedCappedPolicyAckRollupAdjuster(flags, "capped-policy-ack-rollup-adaptive", -6.5),
                new SegmentCappedPolicyAckRollupAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.27, ["central"] = 1.4, ["east"] = 0.87 }),
                new CapCappedPolicyAckRollupAdjuster(8838.0),
            },
            new List<string>(),
            2.0,
            36284.6);
    }

    public double Price(CappedPolicyAckRollup item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedPolicyAckRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
