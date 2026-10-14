namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due baseline calculation.</summary>
public interface ITrainingDueBaselineStep
{
    string Describe();
    double Adjust(TrainingDueBaseline item, double valueValue);
}

public sealed record MinimumTrainingDueBaselineAdjuster(double Minimum) : ITrainingDueBaselineStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentTrainingDueBaselineAdjuster(double Percent) : ITrainingDueBaselineStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTrainingDueBaselineAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueBaselineStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueBaseline item, double valueValue)
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

/// <summary>Runs the training due baseline steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueBaselineEngine(List<ITrainingDueBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueBaselineEngine DefaultTrainingDueBaselineEngine()
    {
        return new TrainingDueBaselineEngine(
            new List<ITrainingDueBaselineStep>
            {
                new MinimumTrainingDueBaselineAdjuster(4.4),
                new PercentTrainingDueBaselineAdjuster(-23.5),
                new TieredTrainingDueBaselineAdjuster(
                    new List<double> { 2975.0, 3960.0 },
                    new List<double> { 1.19, 0.96 }),
            },
            new List<string>(),
            5.0,
            99488.2);
    }

    public double Price(TrainingDueBaseline item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
