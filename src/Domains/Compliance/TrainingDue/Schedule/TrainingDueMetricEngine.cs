namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due metric calculation.</summary>
public interface ITrainingDueMetricModifier
{
    string Describe();
    double Adjust(TrainingDueMetric item, double valueValue);
}

public sealed record TrainingDueMetricWeightOffsetAdjuster(double Weight) : ITrainingDueMetricModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(TrainingDueMetric item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record MinimumTrainingDueMetricAdjuster(double Minimum) : ITrainingDueMetricModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTrainingDueMetricAdjuster(double Cap) : ITrainingDueMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTrainingDueMetricAdjuster(double Percent) : ITrainingDueMetricModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleTrainingDueMetricAdjuster(Dictionary<string, double> Factors) : ITrainingDueMetricModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TrainingDueMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTrainingDueMetricAdjuster(double Step) : ITrainingDueMetricModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the training due metric steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueMetricEngine(List<ITrainingDueMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueMetricEngine DefaultTrainingDueMetricEngine()
    {
        return new TrainingDueMetricEngine(
            new List<ITrainingDueMetricModifier>
            {
                new TrainingDueMetricWeightOffsetAdjuster(1.01),
                new MinimumTrainingDueMetricAdjuster(32.0),
                new CapTrainingDueMetricAdjuster(41883.0),
                new PercentTrainingDueMetricAdjuster(4.4),
                new TitleTrainingDueMetricAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.12, ["online"] = 0.79, ["retail"] = 0.76 }),
                new RoundingTrainingDueMetricAdjuster(1.0),
            },
            new List<string>(),
            0.8,
            67715.4);
    }

    public double Compute(TrainingDueMetric item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
