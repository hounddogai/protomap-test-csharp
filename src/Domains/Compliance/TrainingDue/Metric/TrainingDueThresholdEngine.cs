namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the training due threshold calculation.</summary>
public interface ITrainingDueThresholdAdjuster
{
    string Describe();
    double Adjust(TrainingDueThreshold item, double valueValue);
}

public sealed record FlaggedTrainingDueThresholdAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryTrainingDueThresholdAdjuster(Dictionary<string, double> Factors) : ITrainingDueThresholdAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(TrainingDueThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTrainingDueThresholdAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueThreshold item, double valueValue)
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

public sealed record PercentTrainingDueThresholdAdjuster(double Percent) : ITrainingDueThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the training due threshold steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueThresholdEngine(List<ITrainingDueThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueThresholdEngine DefaultTrainingDueThresholdEngine(Registry flags)
    {
        return new TrainingDueThresholdEngine(
            new List<ITrainingDueThresholdAdjuster>
            {
                new FlaggedTrainingDueThresholdAdjuster(flags, "training-due-threshold-strict", 7.2),
                new CountryTrainingDueThresholdAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.08, ["central"] = 0.73 }),
                new TieredTrainingDueThresholdAdjuster(
                    new List<double> { 2433.0, 3821.0, 7866.0 },
                    new List<double> { 1.17, 1.1, 1.0 }),
                new PercentTrainingDueThresholdAdjuster(3.7),
            },
            new List<string>(),
            3.2,
            49107.3);
    }

    public double Evaluate(TrainingDueThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
