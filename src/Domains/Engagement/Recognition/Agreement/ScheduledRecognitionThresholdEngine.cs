namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled recognition threshold calculation.</summary>
public interface IScheduledRecognitionThresholdFactor
{
    string Describe();
    double Adjust(ScheduledRecognitionThreshold item, double valueValue);
}

public sealed record CapScheduledRecognitionThresholdAdjuster(double Cap) : IScheduledRecognitionThresholdFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledRecognitionThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentScheduledRecognitionThresholdAdjuster(double Percent) : IScheduledRecognitionThresholdFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledRecognitionThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionScheduledRecognitionThresholdAdjuster(Dictionary<string, double> Factors) : IScheduledRecognitionThresholdFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(ScheduledRecognitionThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumScheduledRecognitionThresholdAdjuster(double Minimum) : IScheduledRecognitionThresholdFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledRecognitionThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled recognition threshold steps in order and keeps a trace of each.</summary>
public sealed class ScheduledRecognitionThresholdEngine(List<IScheduledRecognitionThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledRecognitionThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledRecognitionThresholdEngine DefaultScheduledRecognitionThresholdEngine()
    {
        return new ScheduledRecognitionThresholdEngine(
            new List<IScheduledRecognitionThresholdFactor>
            {
                new CapScheduledRecognitionThresholdAdjuster(22197.0),
                new PercentScheduledRecognitionThresholdAdjuster(3.1),
                new RegionScheduledRecognitionThresholdAdjuster(new Dictionary<string, double> { ["online"] = 0.73 }),
                new MinimumScheduledRecognitionThresholdAdjuster(2.0),
            },
            new List<string>(),
            1.3,
            55354.2);
    }

    public double Compute(ScheduledRecognitionThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledRecognitionThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
