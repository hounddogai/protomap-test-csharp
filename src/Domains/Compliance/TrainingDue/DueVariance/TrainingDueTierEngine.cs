namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due tier calculation.</summary>
public interface ITrainingDueTierFactor
{
    string Describe();
    double Adjust(TrainingDueTier item, double valueValue);
}

public sealed record TieredTrainingDueTierAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueTier item, double valueValue)
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

public sealed record RoundingTrainingDueTierAdjuster(double Step) : ITrainingDueTierFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TrainingDueTierUpdatedAtOffsetAdjuster(double Weight) : ITrainingDueTierFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(TrainingDueTier item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record MinimumTrainingDueTierAdjuster(double Minimum) : ITrainingDueTierFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the training due tier steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueTierEngine(List<ITrainingDueTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueTierEngine DefaultTrainingDueTierEngine()
    {
        return new TrainingDueTierEngine(
            new List<ITrainingDueTierFactor>
            {
                new TieredTrainingDueTierAdjuster(
                    new List<double> { 4037.0, 6355.0, 8290.0 },
                    new List<double> { 0.81, 0.86, 1.17 }),
                new RoundingTrainingDueTierAdjuster(0.5),
                new TrainingDueTierUpdatedAtOffsetAdjuster(1.08),
                new MinimumTrainingDueTierAdjuster(39.2),
            },
            new List<string>(),
            4.8,
            71563.3);
    }

    public double Compute(TrainingDueTier item)
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
