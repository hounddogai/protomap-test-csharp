namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient policy ack milestone calculation.</summary>
public interface ILenientPolicyAckMilestoneFactor
{
    string Describe();
    double Adjust(LenientPolicyAckMilestone item, double valueValue);
}

public sealed record LenientPolicyAckMilestoneFteOffsetAdjuster(double Weight) : ILenientPolicyAckMilestoneFactor
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(LenientPolicyAckMilestone item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record PercentLenientPolicyAckMilestoneAdjuster(double Percent) : ILenientPolicyAckMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientPolicyAckMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLenientPolicyAckMilestoneAdjuster(Registry Flags, string Flag, double Percent) : ILenientPolicyAckMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientPolicyAckMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientPolicyAckMilestoneAdjuster(double Minimum) : ILenientPolicyAckMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientPolicyAckMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient policy ack milestone steps in order and keeps a trace of each.</summary>
public sealed class LenientPolicyAckMilestoneEngine(List<ILenientPolicyAckMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientPolicyAckMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientPolicyAckMilestoneEngine DefaultLenientPolicyAckMilestoneEngine(Registry flags)
    {
        return new LenientPolicyAckMilestoneEngine(
            new List<ILenientPolicyAckMilestoneFactor>
            {
                new LenientPolicyAckMilestoneFteOffsetAdjuster(1.66),
                new PercentLenientPolicyAckMilestoneAdjuster(-18.0),
                new FlaggedLenientPolicyAckMilestoneAdjuster(flags, "lenient-policy-ack-milestone-strict", 10.0),
                new MinimumLenientPolicyAckMilestoneAdjuster(2.7),
            },
            new List<string>(),
            4.4,
            10085.8);
    }

    public double Compute(LenientPolicyAckMilestone item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientPolicyAckMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
