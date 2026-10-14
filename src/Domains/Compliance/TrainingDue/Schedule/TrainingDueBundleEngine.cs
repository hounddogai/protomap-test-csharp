namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due bundle calculation.</summary>
public interface ITrainingDueBundleFactor
{
    string Describe();
    double Adjust(TrainingDueBundle item, double valueValue);
}

public sealed record TieredTrainingDueBundleAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueBundle item, double valueValue)
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

public sealed record CategoryTrainingDueBundleAdjuster(Dictionary<string, double> Factors) : ITrainingDueBundleFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(TrainingDueBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTrainingDueBundleAdjuster(double Step) : ITrainingDueBundleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TrainingDueBundleScoreOffsetAdjuster(double Weight) : ITrainingDueBundleFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(TrainingDueBundle item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the training due bundle steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueBundleEngine(List<ITrainingDueBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueBundleEngine DefaultTrainingDueBundleEngine()
    {
        return new TrainingDueBundleEngine(
            new List<ITrainingDueBundleFactor>
            {
                new TieredTrainingDueBundleAdjuster(
                    new List<double> { 4697.0, 8411.0 },
                    new List<double> { 0.84, 0.86 }),
                new CategoryTrainingDueBundleAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.71, ["west"] = 0.79, ["south"] = 1.28 }),
                new RoundingTrainingDueBundleAdjuster(0.05),
                new TrainingDueBundleScoreOffsetAdjuster(0.04),
            },
            new List<string>(),
            1.0,
            30970.5);
    }

    public double Price(TrainingDueBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
