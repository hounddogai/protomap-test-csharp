namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

/// <summary>A rule a scheduled feedback metric must pass. Check returns its problems, or none.</summary>
public interface IScheduledFeedbackMetricRule
{
    List<string> Check(ScheduledFeedbackMetric item);
}

public sealed record ScheduledFeedbackMetricRequiredNameRule(string Message) : IScheduledFeedbackMetricRule
{
    public List<string> Check(ScheduledFeedbackMetric item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFeedbackMetricCodeLengthRule(int MaxLength) : IScheduledFeedbackMetricRule
{
    public List<string> Check(ScheduledFeedbackMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFeedbackMetricNameLengthRule(int MaxLength) : IScheduledFeedbackMetricRule
{
    public List<string> Check(ScheduledFeedbackMetric item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFeedbackMetricRequiredCodeRule(string Message) : IScheduledFeedbackMetricRule
{
    public List<string> Check(ScheduledFeedbackMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFeedbackMetricRatioRangeRule(double Min, double Max) : IScheduledFeedbackMetricRule
{
    public List<string> Check(ScheduledFeedbackMetric item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled feedback metric.</summary>
public sealed record ScheduledFeedbackMetricValidator(List<IScheduledFeedbackMetricRule> Rules)
{
    public List<string> Validate(ScheduledFeedbackMetric item)
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

    public void RequireValid(ScheduledFeedbackMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledFeedbackMetric> items)
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

/// <summary>Functions of scheduled feedback metric rules.</summary>
public static class ScheduledFeedbackMetricRules
{
    public static ScheduledFeedbackMetricValidator DefaultScheduledFeedbackMetricValidator()
    {
        return new ScheduledFeedbackMetricValidator(
            new List<IScheduledFeedbackMetricRule>
            {
                new ScheduledFeedbackMetricRequiredNameRule("scheduled feedback metric needs this field"),
                new ScheduledFeedbackMetricCodeLengthRule(120),
                new ScheduledFeedbackMetricNameLengthRule(64),
                new ScheduledFeedbackMetricRequiredCodeRule("scheduled feedback metric needs this field"),
                new ScheduledFeedbackMetricRatioRangeRule(2.5, 3715.1),
            });
    }
}
