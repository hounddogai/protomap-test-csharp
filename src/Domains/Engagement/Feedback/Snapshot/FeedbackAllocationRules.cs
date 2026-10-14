namespace Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>A rule a feedback allocation must pass. Check returns its problems, or none.</summary>
public interface IFeedbackAllocationRule
{
    List<string> Check(FeedbackAllocation item);
}

public sealed record FeedbackAllocationRequiredTitleRule(string Message) : IFeedbackAllocationRule
{
    public List<string> Check(FeedbackAllocation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackAllocationRequiredCodeRule(string Message) : IFeedbackAllocationRule
{
    public List<string> Check(FeedbackAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackAllocationRequiredReviewerRule(string Message) : IFeedbackAllocationRule
{
    public List<string> Check(FeedbackAllocation item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback allocation.</summary>
public sealed record FeedbackAllocationValidator(List<IFeedbackAllocationRule> Rules)
{
    public List<string> Validate(FeedbackAllocation item)
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

    public void RequireValid(FeedbackAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackAllocation> items)
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

/// <summary>Functions of feedback allocation rules.</summary>
public static class FeedbackAllocationRules
{
    public static FeedbackAllocationValidator DefaultFeedbackAllocationValidator()
    {
        return new FeedbackAllocationValidator(
            new List<IFeedbackAllocationRule>
            {
                new FeedbackAllocationRequiredTitleRule("feedback allocation needs an entry"),
                new FeedbackAllocationRequiredCodeRule("feedback allocation needs this field"),
                new FeedbackAllocationRequiredReviewerRule("feedback allocation needs a value"),
            });
    }
}
