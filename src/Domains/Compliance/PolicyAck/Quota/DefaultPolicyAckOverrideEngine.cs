namespace Acme.Workforce.Compliance.PolicyAck.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default policy ack override calculation.</summary>
public interface IDefaultPolicyAckOverrideModifier
{
    string Describe();
    double Adjust(DefaultPolicyAckOverride item, double valueValue);
}

public sealed record FlaggedDefaultPolicyAckOverrideAdjuster(Registry Flags, string Flag, double Percent) : IDefaultPolicyAckOverrideModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultPolicyAckOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultPolicyAckOverrideAdjuster(double Step) : IDefaultPolicyAckOverrideModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPolicyAckOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultPolicyAckOverrideAdjuster(double Percent) : IDefaultPolicyAckOverrideModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPolicyAckOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelDefaultPolicyAckOverrideAdjuster(Dictionary<string, double> Factors) : IDefaultPolicyAckOverrideModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(DefaultPolicyAckOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDefaultPolicyAckOverrideAdjuster(double Cap) : IDefaultPolicyAckOverrideModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultPolicyAckOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default policy ack override steps in order and keeps a trace of each.</summary>
public sealed class DefaultPolicyAckOverrideEngine(List<IDefaultPolicyAckOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPolicyAckOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPolicyAckOverrideEngine DefaultDefaultPolicyAckOverrideEngine(Registry flags)
    {
        return new DefaultPolicyAckOverrideEngine(
            new List<IDefaultPolicyAckOverrideModifier>
            {
                new FlaggedDefaultPolicyAckOverrideAdjuster(flags, "default-policy-ack-override-manual", 5.8),
                new RoundingDefaultPolicyAckOverrideAdjuster(5.0),
                new PercentDefaultPolicyAckOverrideAdjuster(-0.9),
                new LabelDefaultPolicyAckOverrideAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.08, ["retail"] = 1.31 }),
                new CapDefaultPolicyAckOverrideAdjuster(1489.0),
            },
            new List<string>(),
            3.8,
            8976.7);
    }

    public double Compute(DefaultPolicyAckOverride item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPolicyAckOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
