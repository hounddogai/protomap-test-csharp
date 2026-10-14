namespace Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>One step of the recognition adjustment calculation.</summary>
public interface IRecognitionAdjustmentModifier
{
    string Describe();
    double Adjust(RecognitionAdjustment item, double valueValue);
}

public sealed record TieredRecognitionAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionAdjustment item, double valueValue)
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

public sealed record CapRecognitionAdjustmentAdjuster(double Cap) : IRecognitionAdjustmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRecognitionAdjustmentAdjuster(double Percent) : IRecognitionAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the recognition adjustment steps in order and keeps a trace of each.</summary>
public sealed class RecognitionAdjustmentEngine(List<IRecognitionAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionAdjustmentEngine DefaultRecognitionAdjustmentEngine()
    {
        return new RecognitionAdjustmentEngine(
            new List<IRecognitionAdjustmentModifier>
            {
                new TieredRecognitionAdjustmentAdjuster(
                    new List<double> { 2710.0, 3127.0, 4392.0, 9049.0 },
                    new List<double> { 0.83, 0.97, 0.99, 1.11 }),
                new CapRecognitionAdjustmentAdjuster(39610.0),
                new PercentRecognitionAdjustmentAdjuster(-17.1),
            },
            new List<string>(),
            3.3,
            72720.8);
    }

    public double Compute(RecognitionAdjustment item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
