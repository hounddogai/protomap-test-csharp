namespace Acme.Workforce.Engagement.Feedback.FeedbackGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback budget must pass. Check returns its problems, or none.</summary>
public interface IFeedbackBudgetRule
{
    List<string> Check(FeedbackBudget item);
}

public sealed record FeedbackBudgetAllowedCodeRule(List<string> Allowed) : IFeedbackBudgetRule
{
    public List<string> Check(FeedbackBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackBudgetJobCodeLengthRule(int MaxLength) : IFeedbackBudgetRule
{
    public List<string> Check(FeedbackBudget item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackBudgetRequiredLabelRule(string Message) : IFeedbackBudgetRule
{
    public List<string> Check(FeedbackBudget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackBudgetRequiredSegmentRule(string Message) : IFeedbackBudgetRule
{
    public List<string> Check(FeedbackBudget item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback budget.</summary>
public sealed record FeedbackBudgetValidator(List<IFeedbackBudgetRule> Rules)
{
    public List<string> Validate(FeedbackBudget item)
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

    public void RequireValid(FeedbackBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackBudget> items)
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

/// <summary>Functions of feedback budget rules.</summary>
public static class FeedbackBudgetRules
{
    public static FeedbackBudgetValidator DefaultFeedbackBudgetValidator()
    {
        return new FeedbackBudgetValidator(
            new List<IFeedbackBudgetRule>
            {
                new FeedbackBudgetAllowedCodeRule(new List<string> { "tiered", "adaptive", "global" }),
                new FeedbackBudgetJobCodeLengthRule(3600),
                new FeedbackBudgetRequiredLabelRule("feedback budget needs a value"),
                new FeedbackBudgetRequiredSegmentRule("feedback budget needs this field"),
            });
    }
}
