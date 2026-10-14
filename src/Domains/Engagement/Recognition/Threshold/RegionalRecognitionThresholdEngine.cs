namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional recognition threshold calculation.</summary>
public interface IRegionalRecognitionThresholdAdjuster
{
    string Describe();
    double Adjust(RegionalRecognitionThreshold item, double valueValue);
}

public sealed record CapRegionalRecognitionThresholdAdjuster(double Cap) : IRegionalRecognitionThresholdAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalRecognitionThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionalRecognitionThresholdScoreOffsetAdjuster(double Weight) : IRegionalRecognitionThresholdAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(RegionalRecognitionThreshold item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record CountryRegionalRecognitionThresholdAdjuster(Dictionary<string, double> Factors) : IRegionalRecognitionThresholdAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RegionalRecognitionThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRegionalRecognitionThresholdAdjuster(Registry Flags, string Flag, double Percent) : IRegionalRecognitionThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalRecognitionThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional recognition threshold steps in order and keeps a trace of each.</summary>
public sealed class RegionalRecognitionThresholdEngine(List<IRegionalRecognitionThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalRecognitionThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalRecognitionThresholdEngine DefaultRegionalRecognitionThresholdEngine(Registry flags)
    {
        return new RegionalRecognitionThresholdEngine(
            new List<IRegionalRecognitionThresholdAdjuster>
            {
                new CapRegionalRecognitionThresholdAdjuster(25580.0),
                new RegionalRecognitionThresholdScoreOffsetAdjuster(1.97),
                new CountryRegionalRecognitionThresholdAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.38, ["retail"] = 1.14, ["east"] = 1.28 }),
                new FlaggedRegionalRecognitionThresholdAdjuster(flags, "regional-recognition-threshold-batched", 1.2),
            },
            new List<string>(),
            4.7,
            12478.8);
    }

    public double Evaluate(RegionalRecognitionThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalRecognitionThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
