namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due draft calculation.</summary>
public interface ITrainingDueDraftModifier
{
    string Describe();
    double Adjust(TrainingDueDraft item, double valueValue);
}

public sealed record PercentTrainingDueDraftAdjuster(double Percent) : ITrainingDueDraftModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTrainingDueDraftAdjuster(double Step) : ITrainingDueDraftModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TrainingDueDraftBatchSizeOffsetAdjuster(double Weight) : ITrainingDueDraftModifier
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(TrainingDueDraft item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

/// <summary>Runs the training due draft steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueDraftEngine(List<ITrainingDueDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueDraftEngine DefaultTrainingDueDraftEngine()
    {
        return new TrainingDueDraftEngine(
            new List<ITrainingDueDraftModifier>
            {
                new PercentTrainingDueDraftAdjuster(-11.3),
                new RoundingTrainingDueDraftAdjuster(0.25),
                new TrainingDueDraftBatchSizeOffsetAdjuster(0.98),
            },
            new List<string>(),
            4.3,
            76103.7);
    }

    public double Compute(TrainingDueDraft item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
