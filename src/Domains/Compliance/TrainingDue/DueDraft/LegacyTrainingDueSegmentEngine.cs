namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy training due segment calculation.</summary>
public interface ILegacyTrainingDueSegmentFactor
{
    string Describe();
    double Adjust(LegacyTrainingDueSegment item, double valueValue);
}

public sealed record CapLegacyTrainingDueSegmentAdjuster(double Cap) : ILegacyTrainingDueSegmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyTrainingDueSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLegacyTrainingDueSegmentAdjuster(double Percent) : ILegacyTrainingDueSegmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyTrainingDueSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLegacyTrainingDueSegmentAdjuster(double Step) : ILegacyTrainingDueSegmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyTrainingDueSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLegacyTrainingDueSegmentAdjuster(double Minimum) : ILegacyTrainingDueSegmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyTrainingDueSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLegacyTrainingDueSegmentAdjuster(Registry Flags, string Flag, double Percent) : ILegacyTrainingDueSegmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyTrainingDueSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy training due segment steps in order and keeps a trace of each.</summary>
public sealed class LegacyTrainingDueSegmentEngine(List<ILegacyTrainingDueSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTrainingDueSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTrainingDueSegmentEngine DefaultLegacyTrainingDueSegmentEngine(Registry flags)
    {
        return new LegacyTrainingDueSegmentEngine(
            new List<ILegacyTrainingDueSegmentFactor>
            {
                new CapLegacyTrainingDueSegmentAdjuster(17940.0),
                new PercentLegacyTrainingDueSegmentAdjuster(10.0),
                new RoundingLegacyTrainingDueSegmentAdjuster(5.0),
                new MinimumLegacyTrainingDueSegmentAdjuster(40.9),
                new FlaggedLegacyTrainingDueSegmentAdjuster(flags, "legacy-training-due-segment-pending", -2.3),
            },
            new List<string>(),
            4.8,
            18466.0);
    }

    public double Compute(LegacyTrainingDueSegment item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTrainingDueSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
