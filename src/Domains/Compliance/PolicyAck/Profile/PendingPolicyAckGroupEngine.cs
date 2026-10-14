namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending policy ack group calculation.</summary>
public interface IPendingPolicyAckGroupAdjuster
{
    string Describe();
    double Adjust(PendingPolicyAckGroup item, double valueValue);
}

public sealed record RoundingPendingPolicyAckGroupAdjuster(double Step) : IPendingPolicyAckGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingPolicyAckGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPendingPolicyAckGroupAdjuster(Registry Flags, string Flag, double Percent) : IPendingPolicyAckGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingPolicyAckGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPendingPolicyAckGroupAdjuster(double Percent) : IPendingPolicyAckGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingPolicyAckGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending policy ack group steps in order and keeps a trace of each.</summary>
public sealed class PendingPolicyAckGroupEngine(List<IPendingPolicyAckGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingPolicyAckGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingPolicyAckGroupEngine DefaultPendingPolicyAckGroupEngine(Registry flags)
    {
        return new PendingPolicyAckGroupEngine(
            new List<IPendingPolicyAckGroupAdjuster>
            {
                new RoundingPendingPolicyAckGroupAdjuster(0.25),
                new FlaggedPendingPolicyAckGroupAdjuster(flags, "pending-policy-ack-group-capped", -4.6),
                new PercentPendingPolicyAckGroupAdjuster(17.5),
            },
            new List<string>(),
            0.6,
            55423.6);
    }

    public double Evaluate(PendingPolicyAckGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingPolicyAckGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
