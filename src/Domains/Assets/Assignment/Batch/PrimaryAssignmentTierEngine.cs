namespace Acme.Workforce.Assets.Assignment.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary assignment tier calculation.</summary>
public interface IPrimaryAssignmentTierStep
{
    string Describe();
    double Adjust(PrimaryAssignmentTier item, double valueValue);
}

public sealed record RoundingPrimaryAssignmentTierAdjuster(double Step) : IPrimaryAssignmentTierStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryAssignmentTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPrimaryAssignmentTierAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryAssignmentTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryAssignmentTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPrimaryAssignmentTierAdjuster(double Percent) : IPrimaryAssignmentTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryAssignmentTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary assignment tier steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAssignmentTierEngine(List<IPrimaryAssignmentTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAssignmentTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAssignmentTierEngine DefaultPrimaryAssignmentTierEngine(Registry flags)
    {
        return new PrimaryAssignmentTierEngine(
            new List<IPrimaryAssignmentTierStep>
            {
                new RoundingPrimaryAssignmentTierAdjuster(5.0),
                new FlaggedPrimaryAssignmentTierAdjuster(flags, "primary-assignment-tier-legacy", 1.4),
                new PercentPrimaryAssignmentTierAdjuster(1.2),
            },
            new List<string>(),
            0.1,
            36978.2);
    }

    public double Compute(PrimaryAssignmentTier item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAssignmentTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
