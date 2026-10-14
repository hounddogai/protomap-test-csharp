namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due schedule calculation.</summary>
public interface ITrainingDueScheduleModifier
{
    string Describe();
    double Adjust(TrainingDueSchedule item, double valueValue);
}

public sealed record PercentTrainingDueScheduleAdjuster(double Percent) : ITrainingDueScheduleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTrainingDueScheduleAdjuster(double Cap) : ITrainingDueScheduleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTrainingDueScheduleAdjuster(double Step) : ITrainingDueScheduleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TrainingDueScheduleScoreOffsetAdjuster(double Weight) : ITrainingDueScheduleModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(TrainingDueSchedule item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record MinimumTrainingDueScheduleAdjuster(double Minimum) : ITrainingDueScheduleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTrainingDueScheduleAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueScheduleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the training due schedule steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueScheduleEngine(List<ITrainingDueScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueScheduleEngine DefaultTrainingDueScheduleEngine(Registry flags)
    {
        return new TrainingDueScheduleEngine(
            new List<ITrainingDueScheduleModifier>
            {
                new PercentTrainingDueScheduleAdjuster(22.2),
                new CapTrainingDueScheduleAdjuster(29653.0),
                new RoundingTrainingDueScheduleAdjuster(0.05),
                new TrainingDueScheduleScoreOffsetAdjuster(0.36),
                new MinimumTrainingDueScheduleAdjuster(8.9),
                new FlaggedTrainingDueScheduleAdjuster(flags, "training-due-schedule-fallback", 3.5),
            },
            new List<string>(),
            3.7,
            6408.3);
    }

    public double Compute(TrainingDueSchedule item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
