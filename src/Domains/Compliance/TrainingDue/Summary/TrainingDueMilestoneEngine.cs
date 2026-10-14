namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due milestone calculation.</summary>
public interface ITrainingDueMilestoneModifier
{
    string Describe();
    double Adjust(TrainingDueMilestone item, double valueValue);
}

public sealed record RoundingTrainingDueMilestoneAdjuster(double Step) : ITrainingDueMilestoneModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTrainingDueMilestoneAdjuster(double Cap) : ITrainingDueMilestoneModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTrainingDueMilestoneAdjuster(double Percent) : ITrainingDueMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedTrainingDueMilestoneAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TrainingDueMilestoneScoreOffsetAdjuster(double Weight) : ITrainingDueMilestoneModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(TrainingDueMilestone item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record MinimumTrainingDueMilestoneAdjuster(double Minimum) : ITrainingDueMilestoneModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the training due milestone steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueMilestoneEngine(List<ITrainingDueMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueMilestoneEngine DefaultTrainingDueMilestoneEngine(Registry flags)
    {
        return new TrainingDueMilestoneEngine(
            new List<ITrainingDueMilestoneModifier>
            {
                new RoundingTrainingDueMilestoneAdjuster(0.5),
                new CapTrainingDueMilestoneAdjuster(33953.0),
                new PercentTrainingDueMilestoneAdjuster(-23.1),
                new FlaggedTrainingDueMilestoneAdjuster(flags, "training-due-milestone-batched", -1.0),
                new TrainingDueMilestoneScoreOffsetAdjuster(0.82),
                new MinimumTrainingDueMilestoneAdjuster(27.6),
            },
            new List<string>(),
            3.8,
            47670.8);
    }

    public double Run(TrainingDueMilestone item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
