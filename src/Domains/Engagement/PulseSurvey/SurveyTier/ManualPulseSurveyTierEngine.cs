namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the manual pulse survey tier calculation.</summary>
public interface IManualPulseSurveyTierStep
{
    string Describe();
    double Adjust(ManualPulseSurveyTier item, double valueValue);
}

public sealed record MinimumManualPulseSurveyTierAdjuster(double Minimum) : IManualPulseSurveyTierStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualPulseSurveyTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapManualPulseSurveyTierAdjuster(double Cap) : IManualPulseSurveyTierStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualPulseSurveyTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ManualPulseSurveyTierAmountOffsetAdjuster(double Weight) : IManualPulseSurveyTierStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(ManualPulseSurveyTier item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record PercentManualPulseSurveyTierAdjuster(double Percent) : IManualPulseSurveyTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualPulseSurveyTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedManualPulseSurveyTierAdjuster(Registry Flags, string Flag, double Percent) : IManualPulseSurveyTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualPulseSurveyTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual pulse survey tier steps in order and keeps a trace of each.</summary>
public sealed class ManualPulseSurveyTierEngine(List<IManualPulseSurveyTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualPulseSurveyTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualPulseSurveyTierEngine DefaultManualPulseSurveyTierEngine(Registry flags)
    {
        return new ManualPulseSurveyTierEngine(
            new List<IManualPulseSurveyTierStep>
            {
                new MinimumManualPulseSurveyTierAdjuster(10.8),
                new CapManualPulseSurveyTierAdjuster(31219.0),
                new ManualPulseSurveyTierAmountOffsetAdjuster(0.25),
                new PercentManualPulseSurveyTierAdjuster(19.3),
                new FlaggedManualPulseSurveyTierAdjuster(flags, "manual-pulse-survey-tier-scheduled", 0.7),
            },
            new List<string>(),
            2.6,
            85269.4);
    }

    public double Run(ManualPulseSurveyTier item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualPulseSurveyTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
