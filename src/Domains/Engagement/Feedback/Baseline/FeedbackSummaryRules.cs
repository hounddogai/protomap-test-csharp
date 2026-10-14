namespace Acme.Workforce.Engagement.Feedback.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback summary must pass. Check returns its problems, or none.</summary>
public interface IFeedbackSummaryRule
{
    List<string> Check(FeedbackSummary item);
}

public sealed record FeedbackSummaryCapacityRangeRule(int Min, int Max) : IFeedbackSummaryRule
{
    public List<string> Check(FeedbackSummary item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FeedbackSummaryMarginRangeRule(double Min, double Max) : IFeedbackSummaryRule
{
    public List<string> Check(FeedbackSummary item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FeedbackSummaryRequiredLabelRule(string Message) : IFeedbackSummaryRule
{
    public List<string> Check(FeedbackSummary item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackSummaryAllowedLabelRule(List<string> Allowed) : IFeedbackSummaryRule
{
    public List<string> Check(FeedbackSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is too long" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackSummaryExpiresAtRangeRule(int Min, int Max) : IFeedbackSummaryRule
{
    public List<string> Check(FeedbackSummary item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a feedback summary.</summary>
public sealed record FeedbackSummaryValidator(List<IFeedbackSummaryRule> Rules)
{
    public List<string> Validate(FeedbackSummary item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(FeedbackSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackSummary> items)
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

/// <summary>Functions of feedback summary rules.</summary>
public static class FeedbackSummaryRules
{
    public static FeedbackSummaryValidator DefaultFeedbackSummaryValidator()
    {
        return new FeedbackSummaryValidator(
            new List<IFeedbackSummaryRule>
            {
                new FeedbackSummaryCapacityRangeRule(120, 86400),
                new FeedbackSummaryMarginRangeRule(7.1, 4986.7),
                new FeedbackSummaryRequiredLabelRule("feedback summary needs a value"),
                new FeedbackSummaryAllowedLabelRule(new List<string> { "fallback", "default", "manual" }),
                new FeedbackSummaryExpiresAtRangeRule(32, 3600),
            });
    }
}
