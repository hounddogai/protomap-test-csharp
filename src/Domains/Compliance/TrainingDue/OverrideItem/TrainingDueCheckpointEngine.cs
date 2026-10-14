namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due checkpoint calculation.</summary>
public interface ITrainingDueCheckpointModifier
{
    string Describe();
    double Adjust(TrainingDueCheckpoint item, double valueValue);
}

public sealed record PercentTrainingDueCheckpointAdjuster(double Percent) : ITrainingDueCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTrainingDueCheckpointAdjuster(double Step) : ITrainingDueCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTrainingDueCheckpointAdjuster(double Cap) : ITrainingDueCheckpointModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SegmentTrainingDueCheckpointAdjuster(Dictionary<string, double> Factors) : ITrainingDueCheckpointModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TrainingDueCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTrainingDueCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TrainingDueCheckpointExpiresAtOffsetAdjuster(double Weight) : ITrainingDueCheckpointModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(TrainingDueCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the training due checkpoint steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueCheckpointEngine(List<ITrainingDueCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueCheckpointEngine DefaultTrainingDueCheckpointEngine(Registry flags)
    {
        return new TrainingDueCheckpointEngine(
            new List<ITrainingDueCheckpointModifier>
            {
                new PercentTrainingDueCheckpointAdjuster(24.9),
                new RoundingTrainingDueCheckpointAdjuster(5.0),
                new CapTrainingDueCheckpointAdjuster(46595.0),
                new SegmentTrainingDueCheckpointAdjuster(new Dictionary<string, double> { ["north"] = 0.7 }),
                new FlaggedTrainingDueCheckpointAdjuster(flags, "training-due-checkpoint-scheduled", -5.2),
                new TrainingDueCheckpointExpiresAtOffsetAdjuster(0.94),
            },
            new List<string>(),
            3.1,
            23844.8);
    }

    public double Evaluate(TrainingDueCheckpoint item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
