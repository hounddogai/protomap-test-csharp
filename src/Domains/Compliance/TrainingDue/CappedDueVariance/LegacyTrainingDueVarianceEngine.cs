namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy training due variance calculation.</summary>
public interface ILegacyTrainingDueVarianceFactor
{
    string Describe();
    double Adjust(LegacyTrainingDueVariance item, double valueValue);
}

public sealed record RoundingLegacyTrainingDueVarianceAdjuster(double Step) : ILegacyTrainingDueVarianceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyTrainingDueVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLegacyTrainingDueVarianceAdjuster(double Percent) : ILegacyTrainingDueVarianceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyTrainingDueVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelLegacyTrainingDueVarianceAdjuster(Dictionary<string, double> Factors) : ILegacyTrainingDueVarianceFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LegacyTrainingDueVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the legacy training due variance steps in order and keeps a trace of each.</summary>
public sealed class LegacyTrainingDueVarianceEngine(List<ILegacyTrainingDueVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTrainingDueVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTrainingDueVarianceEngine DefaultLegacyTrainingDueVarianceEngine()
    {
        return new LegacyTrainingDueVarianceEngine(
            new List<ILegacyTrainingDueVarianceFactor>
            {
                new RoundingLegacyTrainingDueVarianceAdjuster(1.0),
                new PercentLegacyTrainingDueVarianceAdjuster(2.6),
                new ChannelLegacyTrainingDueVarianceAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.32, ["south"] = 1.2, ["east"] = 0.85 }),
            },
            new List<string>(),
            4.7,
            38924.9);
    }

    public double Run(LegacyTrainingDueVariance item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTrainingDueVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
