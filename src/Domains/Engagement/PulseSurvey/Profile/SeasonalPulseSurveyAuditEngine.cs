namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal pulse survey audit calculation.</summary>
public interface ISeasonalPulseSurveyAuditAdjuster
{
    string Describe();
    double Adjust(SeasonalPulseSurveyAudit item, double valueValue);
}

public sealed record LabelSeasonalPulseSurveyAuditAdjuster(Dictionary<string, double> Factors) : ISeasonalPulseSurveyAuditAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SeasonalPulseSurveyAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSeasonalPulseSurveyAuditAdjuster(double Percent) : ISeasonalPulseSurveyAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalPulseSurveyAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalPulseSurveyAuditLeaveDaysOffsetAdjuster(double Weight) : ISeasonalPulseSurveyAuditAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(SeasonalPulseSurveyAudit item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

/// <summary>Runs the seasonal pulse survey audit steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPulseSurveyAuditEngine(List<ISeasonalPulseSurveyAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPulseSurveyAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPulseSurveyAuditEngine DefaultSeasonalPulseSurveyAuditEngine()
    {
        return new SeasonalPulseSurveyAuditEngine(
            new List<ISeasonalPulseSurveyAuditAdjuster>
            {
                new LabelSeasonalPulseSurveyAuditAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.1, ["east"] = 0.78 }),
                new PercentSeasonalPulseSurveyAuditAdjuster(15.7),
                new SeasonalPulseSurveyAuditLeaveDaysOffsetAdjuster(1.01),
            },
            new List<string>(),
            1.2,
            25823.9);
    }

    public double Run(SeasonalPulseSurveyAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPulseSurveyAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
