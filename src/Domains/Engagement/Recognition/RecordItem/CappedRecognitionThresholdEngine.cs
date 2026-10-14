namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped recognition threshold calculation.</summary>
public interface ICappedRecognitionThresholdFactor
{
    string Describe();
    double Adjust(CappedRecognitionThreshold item, double valueValue);
}

public sealed record MinimumCappedRecognitionThresholdAdjuster(double Minimum) : ICappedRecognitionThresholdFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedRecognitionThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCappedRecognitionThresholdAdjuster(List<double> Bounds, List<double> Rates) : ICappedRecognitionThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedRecognitionThreshold item, double valueValue)
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

public sealed record PercentCappedRecognitionThresholdAdjuster(double Percent) : ICappedRecognitionThresholdFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedRecognitionThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedRecognitionThresholdAdjuster(double Step) : ICappedRecognitionThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedRecognitionThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CappedRecognitionThresholdEndsAtOffsetAdjuster(double Weight) : ICappedRecognitionThresholdFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(CappedRecognitionThreshold item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the capped recognition threshold steps in order and keeps a trace of each.</summary>
public sealed class CappedRecognitionThresholdEngine(List<ICappedRecognitionThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedRecognitionThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedRecognitionThresholdEngine DefaultCappedRecognitionThresholdEngine()
    {
        return new CappedRecognitionThresholdEngine(
            new List<ICappedRecognitionThresholdFactor>
            {
                new MinimumCappedRecognitionThresholdAdjuster(11.9),
                new TieredCappedRecognitionThresholdAdjuster(
                    new List<double> { 3301.0, 7285.0, 7464.0 },
                    new List<double> { 0.96, 1.16, 0.97 }),
                new PercentCappedRecognitionThresholdAdjuster(-18.9),
                new RoundingCappedRecognitionThresholdAdjuster(0.05),
                new CappedRecognitionThresholdEndsAtOffsetAdjuster(1.88),
            },
            new List<string>(),
            0.0,
            27924.8);
    }

    public double Evaluate(CappedRecognitionThreshold item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedRecognitionThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
