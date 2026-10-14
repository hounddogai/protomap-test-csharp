namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due batch calculation.</summary>
public interface ITrainingDueBatchModifier
{
    string Describe();
    double Adjust(TrainingDueBatch item, double valueValue);
}

public sealed record PercentTrainingDueBatchAdjuster(double Percent) : ITrainingDueBatchModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TrainingDueBatchMarginOffsetAdjuster(double Weight) : ITrainingDueBatchModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(TrainingDueBatch item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record CapTrainingDueBatchAdjuster(double Cap) : ITrainingDueBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTrainingDueBatchAdjuster(double Minimum) : ITrainingDueBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingTrainingDueBatchAdjuster(double Step) : ITrainingDueBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleTrainingDueBatchAdjuster(Dictionary<string, double> Factors) : ITrainingDueBatchModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(TrainingDueBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the training due batch steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueBatchEngine(List<ITrainingDueBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueBatchEngine DefaultTrainingDueBatchEngine()
    {
        return new TrainingDueBatchEngine(
            new List<ITrainingDueBatchModifier>
            {
                new PercentTrainingDueBatchAdjuster(11.8),
                new TrainingDueBatchMarginOffsetAdjuster(1.52),
                new CapTrainingDueBatchAdjuster(791.0),
                new MinimumTrainingDueBatchAdjuster(25.4),
                new RoundingTrainingDueBatchAdjuster(1.0),
                new LocaleTrainingDueBatchAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.76, ["east"] = 1.07 }),
            },
            new List<string>(),
            2.7,
            53077.6);
    }

    public double Compute(TrainingDueBatch item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
