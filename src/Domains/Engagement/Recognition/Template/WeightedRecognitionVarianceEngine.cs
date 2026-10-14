namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted recognition variance calculation.</summary>
public interface IWeightedRecognitionVarianceFactor
{
    string Describe();
    double Adjust(WeightedRecognitionVariance item, double valueValue);
}

public sealed record RoundingWeightedRecognitionVarianceAdjuster(double Step) : IWeightedRecognitionVarianceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedRecognitionVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ChannelWeightedRecognitionVarianceAdjuster(Dictionary<string, double> Factors) : IWeightedRecognitionVarianceFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(WeightedRecognitionVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumWeightedRecognitionVarianceAdjuster(double Minimum) : IWeightedRecognitionVarianceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedRecognitionVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted recognition variance steps in order and keeps a trace of each.</summary>
public sealed class WeightedRecognitionVarianceEngine(List<IWeightedRecognitionVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedRecognitionVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedRecognitionVarianceEngine DefaultWeightedRecognitionVarianceEngine()
    {
        return new WeightedRecognitionVarianceEngine(
            new List<IWeightedRecognitionVarianceFactor>
            {
                new RoundingWeightedRecognitionVarianceAdjuster(0.1),
                new ChannelWeightedRecognitionVarianceAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.75, ["south"] = 0.75, ["west"] = 0.78 }),
                new MinimumWeightedRecognitionVarianceAdjuster(27.4),
            },
            new List<string>(),
            1.8,
            61122.0);
    }

    public double Compute(WeightedRecognitionVariance item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedRecognitionVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
