namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal recognition metric calculation.</summary>
public interface ISeasonalRecognitionMetricStep
{
    string Describe();
    double Adjust(SeasonalRecognitionMetric item, double valueValue);
}

public sealed record SeasonalRecognitionMetricGraceDaysOffsetAdjuster(double Weight) : ISeasonalRecognitionMetricStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(SeasonalRecognitionMetric item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record PercentSeasonalRecognitionMetricAdjuster(double Percent) : ISeasonalRecognitionMetricStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalRecognitionMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalRecognitionMetricAdjuster(double Cap) : ISeasonalRecognitionMetricStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalRecognitionMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSeasonalRecognitionMetricAdjuster(double Step) : ISeasonalRecognitionMetricStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalRecognitionMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the seasonal recognition metric steps in order and keeps a trace of each.</summary>
public sealed class SeasonalRecognitionMetricEngine(List<ISeasonalRecognitionMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalRecognitionMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalRecognitionMetricEngine DefaultSeasonalRecognitionMetricEngine()
    {
        return new SeasonalRecognitionMetricEngine(
            new List<ISeasonalRecognitionMetricStep>
            {
                new SeasonalRecognitionMetricGraceDaysOffsetAdjuster(1.31),
                new PercentSeasonalRecognitionMetricAdjuster(12.0),
                new CapSeasonalRecognitionMetricAdjuster(16983.0),
                new RoundingSeasonalRecognitionMetricAdjuster(1.0),
            },
            new List<string>(),
            4.1,
            63273.5);
    }

    public double Run(SeasonalRecognitionMetric item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalRecognitionMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
