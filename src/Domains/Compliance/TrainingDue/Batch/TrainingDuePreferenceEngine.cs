namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the training due preference calculation.</summary>
public interface ITrainingDuePreferenceFactor
{
    string Describe();
    double Adjust(TrainingDuePreference item, double valueValue);
}

public sealed record FlaggedTrainingDuePreferenceAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDuePreferenceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDuePreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentTrainingDuePreferenceAdjuster(double Percent) : ITrainingDuePreferenceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDuePreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleTrainingDuePreferenceAdjuster(Dictionary<string, double> Factors) : ITrainingDuePreferenceFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TrainingDuePreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the training due preference steps in order and keeps a trace of each.</summary>
public sealed class TrainingDuePreferenceEngine(List<ITrainingDuePreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDuePreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDuePreferenceEngine DefaultTrainingDuePreferenceEngine(Registry flags)
    {
        return new TrainingDuePreferenceEngine(
            new List<ITrainingDuePreferenceFactor>
            {
                new FlaggedTrainingDuePreferenceAdjuster(flags, "training-due-preference-tiered", 2.7),
                new PercentTrainingDuePreferenceAdjuster(-24.2),
                new TitleTrainingDuePreferenceAdjuster(new Dictionary<string, double> { ["east"] = 0.82 }),
            },
            new List<string>(),
            1.3,
            68533.4);
    }

    public double Evaluate(TrainingDuePreference item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDuePreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
