namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>A rule a scheduled pulse survey metric must pass. Check returns its problems, or none.</summary>
public interface IScheduledPulseSurveyMetricRule
{
    List<string> Check(ScheduledPulseSurveyMetric item);
}

public sealed record ScheduledPulseSurveyMetricRequiredLocationRule(string Message) : IScheduledPulseSurveyMetricRule
{
    public List<string> Check(ScheduledPulseSurveyMetric item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPulseSurveyMetricUnitLengthRule(int MaxLength) : IScheduledPulseSurveyMetricRule
{
    public List<string> Check(ScheduledPulseSurveyMetric item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPulseSurveyMetricDurationMinutesRangeRule(int Min, int Max) : IScheduledPulseSurveyMetricRule
{
    public List<string> Check(ScheduledPulseSurveyMetric item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledPulseSurveyMetricRequiredTitleRule(string Message) : IScheduledPulseSurveyMetricRule
{
    public List<string> Check(ScheduledPulseSurveyMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pulse survey metric.</summary>
public sealed record ScheduledPulseSurveyMetricValidator(List<IScheduledPulseSurveyMetricRule> Rules)
{
    public List<string> Validate(ScheduledPulseSurveyMetric item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(ScheduledPulseSurveyMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPulseSurveyMetric> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of scheduled pulse survey metric rules.</summary>
public static class ScheduledPulseSurveyMetricRules
{
    public static ScheduledPulseSurveyMetricValidator DefaultScheduledPulseSurveyMetricValidator()
    {
        return new ScheduledPulseSurveyMetricValidator(
            new List<IScheduledPulseSurveyMetricRule>
            {
                new ScheduledPulseSurveyMetricRequiredLocationRule("scheduled pulse survey metric needs an entry"),
                new ScheduledPulseSurveyMetricUnitLengthRule(120),
                new ScheduledPulseSurveyMetricDurationMinutesRangeRule(80, 255),
                new ScheduledPulseSurveyMetricRequiredTitleRule("scheduled pulse survey metric needs a value"),
            });
    }
}
