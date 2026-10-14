namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback policy ack snapshot calculation.</summary>
public interface IFallbackPolicyAckSnapshotStep
{
    string Describe();
    double Adjust(FallbackPolicyAckSnapshot item, double valueValue);
}

public sealed record FlaggedFallbackPolicyAckSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IFallbackPolicyAckSnapshotStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackPolicyAckSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFallbackPolicyAckSnapshotAdjuster(double Percent) : IFallbackPolicyAckSnapshotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackPolicyAckSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackPolicyAckSnapshotThresholdOffsetAdjuster(double Weight) : IFallbackPolicyAckSnapshotStep
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(FallbackPolicyAckSnapshot item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the fallback policy ack snapshot steps in order and keeps a trace of each.</summary>
public sealed class FallbackPolicyAckSnapshotEngine(List<IFallbackPolicyAckSnapshotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPolicyAckSnapshotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPolicyAckSnapshotEngine DefaultFallbackPolicyAckSnapshotEngine(Registry flags)
    {
        return new FallbackPolicyAckSnapshotEngine(
            new List<IFallbackPolicyAckSnapshotStep>
            {
                new FlaggedFallbackPolicyAckSnapshotAdjuster(flags, "fallback-policy-ack-snapshot-pending", -3.5),
                new PercentFallbackPolicyAckSnapshotAdjuster(-21.5),
                new FallbackPolicyAckSnapshotThresholdOffsetAdjuster(0.62),
            },
            new List<string>(),
            5.0,
            66878.1);
    }

    public double Compute(FallbackPolicyAckSnapshot item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPolicyAckSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
