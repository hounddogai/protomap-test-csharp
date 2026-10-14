namespace Acme.Workforce.Engagement.PulseSurvey.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual pulse survey quota calculation.</summary>
public interface IManualPulseSurveyQuotaAdjuster
{
    string Describe();
    double Adjust(ManualPulseSurveyQuota item, double valueValue);
}

public sealed record PercentManualPulseSurveyQuotaAdjuster(double Percent) : IManualPulseSurveyQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualPulseSurveyQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualPulseSurveyQuotaAdjuster(double Cap) : IManualPulseSurveyQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualPulseSurveyQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumManualPulseSurveyQuotaAdjuster(double Minimum) : IManualPulseSurveyQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualPulseSurveyQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingManualPulseSurveyQuotaAdjuster(double Step) : IManualPulseSurveyQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualPulseSurveyQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedManualPulseSurveyQuotaAdjuster(Registry Flags, string Flag, double Percent) : IManualPulseSurveyQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualPulseSurveyQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual pulse survey quota steps in order and keeps a trace of each.</summary>
public sealed class ManualPulseSurveyQuotaEngine(List<IManualPulseSurveyQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualPulseSurveyQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualPulseSurveyQuotaEngine DefaultManualPulseSurveyQuotaEngine(Registry flags)
    {
        return new ManualPulseSurveyQuotaEngine(
            new List<IManualPulseSurveyQuotaAdjuster>
            {
                new PercentManualPulseSurveyQuotaAdjuster(-23.9),
                new CapManualPulseSurveyQuotaAdjuster(18632.0),
                new MinimumManualPulseSurveyQuotaAdjuster(41.4),
                new RoundingManualPulseSurveyQuotaAdjuster(5.0),
                new FlaggedManualPulseSurveyQuotaAdjuster(flags, "manual-pulse-survey-quota-batched", -4.9),
            },
            new List<string>(),
            4.7,
            4611.4);
    }

    public double Run(ManualPulseSurveyQuota item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualPulseSurveyQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
