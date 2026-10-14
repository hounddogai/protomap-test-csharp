namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled pulse survey adjustment calculation.</summary>
public interface IScheduledPulseSurveyAdjustmentModifier
{
    string Describe();
    double Adjust(ScheduledPulseSurveyAdjustment item, double valueValue);
}

public sealed record CapScheduledPulseSurveyAdjustmentAdjuster(double Cap) : IScheduledPulseSurveyAdjustmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledPulseSurveyAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingScheduledPulseSurveyAdjustmentAdjuster(double Step) : IScheduledPulseSurveyAdjustmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledPulseSurveyAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumScheduledPulseSurveyAdjustmentAdjuster(double Minimum) : IScheduledPulseSurveyAdjustmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledPulseSurveyAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled pulse survey adjustment steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPulseSurveyAdjustmentEngine(List<IScheduledPulseSurveyAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPulseSurveyAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPulseSurveyAdjustmentEngine DefaultScheduledPulseSurveyAdjustmentEngine()
    {
        return new ScheduledPulseSurveyAdjustmentEngine(
            new List<IScheduledPulseSurveyAdjustmentModifier>
            {
                new CapScheduledPulseSurveyAdjustmentAdjuster(22539.0),
                new RoundingScheduledPulseSurveyAdjustmentAdjuster(0.1),
                new MinimumScheduledPulseSurveyAdjustmentAdjuster(29.1),
            },
            new List<string>(),
            0.7,
            15818.3);
    }

    public double Price(ScheduledPulseSurveyAdjustment item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPulseSurveyAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
