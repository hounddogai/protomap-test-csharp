namespace Acme.Workforce.Compliance.PolicyAck.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered policy ack override calculation.</summary>
public interface ITieredPolicyAckOverrideModifier
{
    string Describe();
    double Adjust(TieredPolicyAckOverride item, double valueValue);
}

public sealed record PercentTieredPolicyAckOverrideAdjuster(double Percent) : ITieredPolicyAckOverrideModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredPolicyAckOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredPolicyAckOverrideAdjuster(double Step) : ITieredPolicyAckOverrideModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredPolicyAckOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTieredPolicyAckOverrideAdjuster(Registry Flags, string Flag, double Percent) : ITieredPolicyAckOverrideModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredPolicyAckOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered policy ack override steps in order and keeps a trace of each.</summary>
public sealed class TieredPolicyAckOverrideEngine(List<ITieredPolicyAckOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPolicyAckOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPolicyAckOverrideEngine DefaultTieredPolicyAckOverrideEngine(Registry flags)
    {
        return new TieredPolicyAckOverrideEngine(
            new List<ITieredPolicyAckOverrideModifier>
            {
                new PercentTieredPolicyAckOverrideAdjuster(6.3),
                new RoundingTieredPolicyAckOverrideAdjuster(0.25),
                new FlaggedTieredPolicyAckOverrideAdjuster(flags, "tiered-policy-ack-override-rolling", 8.9),
            },
            new List<string>(),
            1.9,
            28721.2);
    }

    public double Compute(TieredPolicyAckOverride item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredPolicyAckOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
