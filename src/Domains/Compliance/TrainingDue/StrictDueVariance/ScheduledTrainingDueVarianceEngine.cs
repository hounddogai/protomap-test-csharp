namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled training due variance calculation.</summary>
public interface IScheduledTrainingDueVarianceStep
{
    string Describe();
    double Adjust(ScheduledTrainingDueVariance item, double valueValue);
}

public sealed record FlaggedScheduledTrainingDueVarianceAdjuster(Registry Flags, string Flag, double Percent) : IScheduledTrainingDueVarianceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledTrainingDueVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledTrainingDueVarianceAdjuster(double Step) : IScheduledTrainingDueVarianceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledTrainingDueVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentScheduledTrainingDueVarianceAdjuster(double Percent) : IScheduledTrainingDueVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledTrainingDueVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled training due variance steps in order and keeps a trace of each.</summary>
public sealed class ScheduledTrainingDueVarianceEngine(List<IScheduledTrainingDueVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledTrainingDueVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledTrainingDueVarianceEngine DefaultScheduledTrainingDueVarianceEngine(Registry flags)
    {
        return new ScheduledTrainingDueVarianceEngine(
            new List<IScheduledTrainingDueVarianceStep>
            {
                new FlaggedScheduledTrainingDueVarianceAdjuster(
                    flags,
                    "scheduled-training-due-variance-manual",
                    -0.4),
                new RoundingScheduledTrainingDueVarianceAdjuster(5.0),
                new PercentScheduledTrainingDueVarianceAdjuster(-8.9),
            },
            new List<string>(),
            4.5,
            3590.0);
    }

    public double Compute(ScheduledTrainingDueVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledTrainingDueVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
