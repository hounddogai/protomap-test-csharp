namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback metric must pass. Check returns its problems, or none.</summary>
public interface IFeedbackMetricRule
{
    List<string> Check(FeedbackMetric item);
}

public sealed record FeedbackMetricAllowedSummaryRule(List<string> Allowed) : IFeedbackMetricRule
{
    public List<string> Check(FeedbackMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is too long" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackMetricTitleLengthRule(int MaxLength) : IFeedbackMetricRule
{
    public List<string> Check(FeedbackMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackMetricCategoryLengthRule(int MaxLength) : IFeedbackMetricRule
{
    public List<string> Check(FeedbackMetric item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback metric.</summary>
public sealed record FeedbackMetricValidator(List<IFeedbackMetricRule> Rules)
{
    public List<string> Validate(FeedbackMetric item)
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

    public void RequireValid(FeedbackMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackMetric> items)
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

/// <summary>Functions of feedback metric rules.</summary>
public static class FeedbackMetricRules
{
    public static FeedbackMetricValidator DefaultFeedbackMetricValidator()
    {
        return new FeedbackMetricValidator(
            new List<IFeedbackMetricRule>
            {
                new FeedbackMetricAllowedSummaryRule(new List<string> { "pending" }),
                new FeedbackMetricTitleLengthRule(120),
                new FeedbackMetricCategoryLengthRule(86400),
            });
    }
}
