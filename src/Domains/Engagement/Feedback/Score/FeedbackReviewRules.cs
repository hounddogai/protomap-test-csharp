namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback review must pass. Check returns its problems, or none.</summary>
public interface IFeedbackReviewRule
{
    List<string> Check(FeedbackReview item);
}

public sealed record FeedbackReviewTitleLengthRule(int MaxLength) : IFeedbackReviewRule
{
    public List<string> Check(FeedbackReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackReviewCodeLengthRule(int MaxLength) : IFeedbackReviewRule
{
    public List<string> Check(FeedbackReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackReviewEmployeeIdLengthRule(int MaxLength) : IFeedbackReviewRule
{
    public List<string> Check(FeedbackReview item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackReviewAllowedOwnerIdRule(List<string> Allowed) : IFeedbackReviewRule
{
    public List<string> Check(FeedbackReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackReviewRequiredEmployeeIdRule(string Message) : IFeedbackReviewRule
{
    public List<string> Check(FeedbackReview item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackReviewBudgetRangeRule(double Min, double Max) : IFeedbackReviewRule
{
    public List<string> Check(FeedbackReview item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a feedback review.</summary>
public sealed record FeedbackReviewValidator(List<IFeedbackReviewRule> Rules)
{
    public List<string> Validate(FeedbackReview item)
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

    public void RequireValid(FeedbackReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackReview> items)
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

/// <summary>Functions of feedback review rules.</summary>
public static class FeedbackReviewRules
{
    public static FeedbackReviewValidator DefaultFeedbackReviewValidator()
    {
        return new FeedbackReviewValidator(
            new List<IFeedbackReviewRule>
            {
                new FeedbackReviewTitleLengthRule(64),
                new FeedbackReviewCodeLengthRule(120),
                new FeedbackReviewEmployeeIdLengthRule(1000),
                new FeedbackReviewAllowedOwnerIdRule(new List<string> { "adaptive" }),
                new FeedbackReviewRequiredEmployeeIdRule("feedback review needs a value"),
                new FeedbackReviewBudgetRangeRule(5.9, 2309.0),
            });
    }
}
