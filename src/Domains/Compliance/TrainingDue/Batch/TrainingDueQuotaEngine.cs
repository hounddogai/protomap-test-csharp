namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due quota calculation.</summary>
public interface ITrainingDueQuotaModifier
{
    string Describe();
    double Adjust(TrainingDueQuota item, double valueValue);
}

public sealed record ChannelTrainingDueQuotaAdjuster(Dictionary<string, double> Factors) : ITrainingDueQuotaModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(TrainingDueQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTrainingDueQuotaAdjuster(double Step) : ITrainingDueQuotaModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TrainingDueQuotaMinimumOffsetAdjuster(double Weight) : ITrainingDueQuotaModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(TrainingDueQuota item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the training due quota steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueQuotaEngine(List<ITrainingDueQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueQuotaEngine DefaultTrainingDueQuotaEngine()
    {
        return new TrainingDueQuotaEngine(
            new List<ITrainingDueQuotaModifier>
            {
                new ChannelTrainingDueQuotaAdjuster(new Dictionary<string, double> { ["north"] = 0.98 }),
                new RoundingTrainingDueQuotaAdjuster(0.5),
                new TrainingDueQuotaMinimumOffsetAdjuster(1.67),
            },
            new List<string>(),
            5.0,
            5345.7);
    }

    public double Compute(TrainingDueQuota item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
