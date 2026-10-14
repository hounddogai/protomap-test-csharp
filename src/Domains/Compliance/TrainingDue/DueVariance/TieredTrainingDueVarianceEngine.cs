namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered training due variance calculation.</summary>
public interface ITieredTrainingDueVarianceStep
{
    string Describe();
    double Adjust(TieredTrainingDueVariance item, double valueValue);
}

public sealed record PercentTieredTrainingDueVarianceAdjuster(double Percent) : ITieredTrainingDueVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredTrainingDueVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredTrainingDueVarianceAdjuster(double Cap) : ITieredTrainingDueVarianceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredTrainingDueVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTrainingDueVarianceBudgetOffsetAdjuster(double Weight) : ITieredTrainingDueVarianceStep
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(TieredTrainingDueVariance item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record SegmentTieredTrainingDueVarianceAdjuster(Dictionary<string, double> Factors) : ITieredTrainingDueVarianceStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TieredTrainingDueVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTieredTrainingDueVarianceAdjuster(Registry Flags, string Flag, double Percent) : ITieredTrainingDueVarianceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredTrainingDueVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered training due variance steps in order and keeps a trace of each.</summary>
public sealed class TieredTrainingDueVarianceEngine(List<ITieredTrainingDueVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredTrainingDueVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredTrainingDueVarianceEngine DefaultTieredTrainingDueVarianceEngine(Registry flags)
    {
        return new TieredTrainingDueVarianceEngine(
            new List<ITieredTrainingDueVarianceStep>
            {
                new PercentTieredTrainingDueVarianceAdjuster(-4.5),
                new CapTieredTrainingDueVarianceAdjuster(17036.0),
                new TieredTrainingDueVarianceBudgetOffsetAdjuster(0.07),
                new SegmentTieredTrainingDueVarianceAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.35, ["online"] = 1.02 }),
                new FlaggedTieredTrainingDueVarianceAdjuster(flags, "tiered-training-due-variance-manual", 4.9),
            },
            new List<string>(),
            1.9,
            21738.7);
    }

    public double Compute(TieredTrainingDueVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredTrainingDueVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
