namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal training due metric calculation.</summary>
public interface ISeasonalTrainingDueMetricModifier
{
    string Describe();
    double Adjust(SeasonalTrainingDueMetric item, double valueValue);
}

public sealed record FlaggedSeasonalTrainingDueMetricAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalTrainingDueMetricModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalTrainingDueMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSeasonalTrainingDueMetricAdjuster(double Percent) : ISeasonalTrainingDueMetricModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalTrainingDueMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalTrainingDueMetricQuantityOffsetAdjuster(double Weight) : ISeasonalTrainingDueMetricModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(SeasonalTrainingDueMetric item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record TitleSeasonalTrainingDueMetricAdjuster(Dictionary<string, double> Factors) : ISeasonalTrainingDueMetricModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SeasonalTrainingDueMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSeasonalTrainingDueMetricAdjuster(double Cap) : ISeasonalTrainingDueMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalTrainingDueMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal training due metric steps in order and keeps a trace of each.</summary>
public sealed class SeasonalTrainingDueMetricEngine(List<ISeasonalTrainingDueMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalTrainingDueMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalTrainingDueMetricEngine DefaultSeasonalTrainingDueMetricEngine(Registry flags)
    {
        return new SeasonalTrainingDueMetricEngine(
            new List<ISeasonalTrainingDueMetricModifier>
            {
                new FlaggedSeasonalTrainingDueMetricAdjuster(flags, "seasonal-training-due-metric-default", 4.6),
                new PercentSeasonalTrainingDueMetricAdjuster(-17.9),
                new SeasonalTrainingDueMetricQuantityOffsetAdjuster(1.41),
                new TitleSeasonalTrainingDueMetricAdjuster(new Dictionary<string, double> { ["north"] = 1.19 }),
                new CapSeasonalTrainingDueMetricAdjuster(25316.0),
            },
            new List<string>(),
            0.0,
            65925.2);
    }

    public double Compute(SeasonalTrainingDueMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalTrainingDueMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
