namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted recognition threshold calculation.</summary>
public interface IWeightedRecognitionThresholdStep
{
    string Describe();
    double Adjust(WeightedRecognitionThreshold item, double valueValue);
}

public sealed record RoundingWeightedRecognitionThresholdAdjuster(double Step) : IWeightedRecognitionThresholdStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedRecognitionThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WeightedRecognitionThresholdCreatedAtOffsetAdjuster(double Weight) : IWeightedRecognitionThresholdStep
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(WeightedRecognitionThreshold item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record FlaggedWeightedRecognitionThresholdAdjuster(Registry Flags, string Flag, double Percent) : IWeightedRecognitionThresholdStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedRecognitionThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedRecognitionThresholdAdjuster(List<double> Bounds, List<double> Rates) : IWeightedRecognitionThresholdStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedRecognitionThreshold item, double valueValue)
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

/// <summary>Runs the weighted recognition threshold steps in order and keeps a trace of each.</summary>
public sealed class WeightedRecognitionThresholdEngine(List<IWeightedRecognitionThresholdStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedRecognitionThresholdStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedRecognitionThresholdEngine DefaultWeightedRecognitionThresholdEngine(Registry flags)
    {
        return new WeightedRecognitionThresholdEngine(
            new List<IWeightedRecognitionThresholdStep>
            {
                new RoundingWeightedRecognitionThresholdAdjuster(0.5),
                new WeightedRecognitionThresholdCreatedAtOffsetAdjuster(0.24),
                new FlaggedWeightedRecognitionThresholdAdjuster(flags, "weighted-recognition-threshold-global", 2.5),
                new TieredWeightedRecognitionThresholdAdjuster(
                    new List<double> { 2775.0, 3109.0, 7322.0, 8976.0 },
                    new List<double> { 1.17, 1.18, 1.12, 1.05 }),
            },
            new List<string>(),
            0.7,
            95862.3);
    }

    public double Run(WeightedRecognitionThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedRecognitionThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
