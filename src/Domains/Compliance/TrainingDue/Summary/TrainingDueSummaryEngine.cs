namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the training due summary calculation.</summary>
public interface ITrainingDueSummaryStep
{
    string Describe();
    double Adjust(TrainingDueSummary item, double valueValue);
}

public sealed record PercentTrainingDueSummaryAdjuster(double Percent) : ITrainingDueSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameTrainingDueSummaryAdjuster(Dictionary<string, double> Factors) : ITrainingDueSummaryStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TrainingDueSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTrainingDueSummaryAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueSummary item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record FlaggedTrainingDueSummaryAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTrainingDueSummaryAdjuster(double Minimum) : ITrainingDueSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the training due summary steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueSummaryEngine(List<ITrainingDueSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueSummaryEngine DefaultTrainingDueSummaryEngine(Registry flags)
    {
        return new TrainingDueSummaryEngine(
            new List<ITrainingDueSummaryStep>
            {
                new PercentTrainingDueSummaryAdjuster(-9.5),
                new NameTrainingDueSummaryAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.35, ["retail"] = 0.84 }),
                new TieredTrainingDueSummaryAdjuster(
                    new List<double> { 4218.0, 4435.0, 5174.0, 9962.0 },
                    new List<double> { 1.12, 0.94, 0.95, 0.88 }),
                new FlaggedTrainingDueSummaryAdjuster(flags, "training-due-summary-regional", -6.5),
                new MinimumTrainingDueSummaryAdjuster(11.5),
            },
            new List<string>(),
            3.5,
            25062.7);
    }

    public double Compute(TrainingDueSummary item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
