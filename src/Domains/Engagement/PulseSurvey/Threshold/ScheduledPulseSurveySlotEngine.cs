namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>One step of the scheduled pulse survey slot calculation.</summary>
public interface IScheduledPulseSurveySlotFactor
{
    string Describe();
    double Adjust(ScheduledPulseSurveySlot item, double valueValue);
}

public sealed record ScheduledPulseSurveySlotRatioOffsetAdjuster(double Weight) : IScheduledPulseSurveySlotFactor
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(ScheduledPulseSurveySlot item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record CapScheduledPulseSurveySlotAdjuster(double Cap) : IScheduledPulseSurveySlotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledPulseSurveySlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentScheduledPulseSurveySlotAdjuster(double Percent) : IScheduledPulseSurveySlotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledPulseSurveySlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled pulse survey slot steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPulseSurveySlotEngine(List<IScheduledPulseSurveySlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPulseSurveySlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPulseSurveySlotEngine DefaultScheduledPulseSurveySlotEngine()
    {
        return new ScheduledPulseSurveySlotEngine(
            new List<IScheduledPulseSurveySlotFactor>
            {
                new ScheduledPulseSurveySlotRatioOffsetAdjuster(1.42),
                new CapScheduledPulseSurveySlotAdjuster(32227.0),
                new PercentScheduledPulseSurveySlotAdjuster(24.1),
            },
            new List<string>(),
            4.2,
            87851.6);
    }

    public double Compute(ScheduledPulseSurveySlot item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPulseSurveySlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
