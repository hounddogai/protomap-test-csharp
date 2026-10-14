namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive recognition batch calculation.</summary>
public interface IAdaptiveRecognitionBatchModifier
{
    string Describe();
    double Adjust(AdaptiveRecognitionBatch item, double valueValue);
}

public sealed record AdaptiveRecognitionBatchMarginOffsetAdjuster(double Weight) : IAdaptiveRecognitionBatchModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(AdaptiveRecognitionBatch item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record CapAdaptiveRecognitionBatchAdjuster(double Cap) : IAdaptiveRecognitionBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveRecognitionBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAdaptiveRecognitionBatchAdjuster(double Step) : IAdaptiveRecognitionBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveRecognitionBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAdaptiveRecognitionBatchAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveRecognitionBatchModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveRecognitionBatch item, double valueValue)
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

public sealed record MinimumAdaptiveRecognitionBatchAdjuster(double Minimum) : IAdaptiveRecognitionBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveRecognitionBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TeamAdaptiveRecognitionBatchAdjuster(Dictionary<string, double> Factors) : IAdaptiveRecognitionBatchModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(AdaptiveRecognitionBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive recognition batch steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveRecognitionBatchEngine(List<IAdaptiveRecognitionBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveRecognitionBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveRecognitionBatchEngine DefaultAdaptiveRecognitionBatchEngine()
    {
        return new AdaptiveRecognitionBatchEngine(
            new List<IAdaptiveRecognitionBatchModifier>
            {
                new AdaptiveRecognitionBatchMarginOffsetAdjuster(0.76),
                new CapAdaptiveRecognitionBatchAdjuster(975.0),
                new RoundingAdaptiveRecognitionBatchAdjuster(1.0),
                new TieredAdaptiveRecognitionBatchAdjuster(
                    new List<double> { 6375.0, 9518.0 },
                    new List<double> { 1.09, 1.13 }),
                new MinimumAdaptiveRecognitionBatchAdjuster(49.4),
                new TeamAdaptiveRecognitionBatchAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.03, ["east"] = 1.04 }),
            },
            new List<string>(),
            4.3,
            29422.2);
    }

    public double Price(AdaptiveRecognitionBatch item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveRecognitionBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
