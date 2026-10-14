namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted sentiment manifest calculation.</summary>
public interface IWeightedSentimentManifestStep
{
    string Describe();
    double Adjust(WeightedSentimentManifest item, double valueValue);
}

public sealed record DepartmentWeightedSentimentManifestAdjuster(Dictionary<string, double> Factors) : IWeightedSentimentManifestStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(WeightedSentimentManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentWeightedSentimentManifestAdjuster(double Percent) : IWeightedSentimentManifestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedSentimentManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedSentimentManifestAdjuster(double Cap) : IWeightedSentimentManifestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedSentimentManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the weighted sentiment manifest steps in order and keeps a trace of each.</summary>
public sealed class WeightedSentimentManifestEngine(List<IWeightedSentimentManifestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedSentimentManifestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedSentimentManifestEngine DefaultWeightedSentimentManifestEngine()
    {
        return new WeightedSentimentManifestEngine(
            new List<IWeightedSentimentManifestStep>
            {
                new DepartmentWeightedSentimentManifestAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.38, ["online"] = 1.27 }),
                new PercentWeightedSentimentManifestAdjuster(12.5),
                new CapWeightedSentimentManifestAdjuster(36886.0),
            },
            new List<string>(),
            3.3,
            13859.8);
    }

    public double Run(WeightedSentimentManifest item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedSentimentManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
