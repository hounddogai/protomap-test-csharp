namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient policy ack cycle calculation.</summary>
public interface ILenientPolicyAckCycleFactor
{
    string Describe();
    double Adjust(LenientPolicyAckCycle item, double valueValue);
}

public sealed record MinimumLenientPolicyAckCycleAdjuster(double Minimum) : ILenientPolicyAckCycleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientPolicyAckCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientPolicyAckCycleAdjuster(Registry Flags, string Flag, double Percent) : ILenientPolicyAckCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientPolicyAckCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientPolicyAckCycleCreatedAtOffsetAdjuster(double Weight) : ILenientPolicyAckCycleFactor
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(LenientPolicyAckCycle item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record PercentLenientPolicyAckCycleAdjuster(double Percent) : ILenientPolicyAckCycleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientPolicyAckCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient policy ack cycle steps in order and keeps a trace of each.</summary>
public sealed class LenientPolicyAckCycleEngine(List<ILenientPolicyAckCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientPolicyAckCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientPolicyAckCycleEngine DefaultLenientPolicyAckCycleEngine(Registry flags)
    {
        return new LenientPolicyAckCycleEngine(
            new List<ILenientPolicyAckCycleFactor>
            {
                new MinimumLenientPolicyAckCycleAdjuster(12.7),
                new FlaggedLenientPolicyAckCycleAdjuster(flags, "lenient-policy-ack-cycle-capped", 8.2),
                new LenientPolicyAckCycleCreatedAtOffsetAdjuster(0.23),
                new PercentLenientPolicyAckCycleAdjuster(-2.5),
            },
            new List<string>(),
            3.6,
            86377.8);
    }

    public double Evaluate(LenientPolicyAckCycle item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientPolicyAckCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
