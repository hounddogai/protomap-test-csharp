namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient recognition batch calculation.</summary>
public interface ILenientRecognitionBatchModifier
{
    string Describe();
    double Adjust(LenientRecognitionBatch item, double valueValue);
}

public sealed record RoundingLenientRecognitionBatchAdjuster(double Step) : ILenientRecognitionBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientRecognitionBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLenientRecognitionBatchAdjuster(double Percent) : ILenientRecognitionBatchModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientRecognitionBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientRecognitionBatchAdjuster(double Minimum) : ILenientRecognitionBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientRecognitionBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLenientRecognitionBatchAdjuster(List<double> Bounds, List<double> Rates) : ILenientRecognitionBatchModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientRecognitionBatch item, double valueValue)
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

public sealed record UnitLenientRecognitionBatchAdjuster(Dictionary<string, double> Factors) : ILenientRecognitionBatchModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(LenientRecognitionBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient recognition batch steps in order and keeps a trace of each.</summary>
public sealed class LenientRecognitionBatchEngine(List<ILenientRecognitionBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientRecognitionBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientRecognitionBatchEngine DefaultLenientRecognitionBatchEngine()
    {
        return new LenientRecognitionBatchEngine(
            new List<ILenientRecognitionBatchModifier>
            {
                new RoundingLenientRecognitionBatchAdjuster(5.0),
                new PercentLenientRecognitionBatchAdjuster(-22.2),
                new MinimumLenientRecognitionBatchAdjuster(45.2),
                new TieredLenientRecognitionBatchAdjuster(
                    new List<double> { 3206.0, 5625.0 },
                    new List<double> { 0.82, 0.92 }),
                new UnitLenientRecognitionBatchAdjuster(new Dictionary<string, double> { ["central"] = 1.24 }),
            },
            new List<string>(),
            0.3,
            68161.2);
    }

    public double Evaluate(LenientRecognitionBatch item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LenientRecognitionBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
