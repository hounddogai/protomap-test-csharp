namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled engagement review must pass. Check returns its problems, or none.</summary>
public interface IScheduledEngagementReviewRule
{
    List<string> Check(ScheduledEngagementReview item);
}

public sealed record ScheduledEngagementReviewRequiredChannelRule(string Message) : IScheduledEngagementReviewRule
{
    public List<string> Check(ScheduledEngagementReview item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementReviewRequiredSourceRule(string Message) : IScheduledEngagementReviewRule
{
    public List<string> Check(ScheduledEngagementReview item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementReviewRequiredCodeRule(string Message) : IScheduledEngagementReviewRule
{
    public List<string> Check(ScheduledEngagementReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementReviewRequiredLabelRule(string Message) : IScheduledEngagementReviewRule
{
    public List<string> Check(ScheduledEngagementReview item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementReviewAllowedCodeRule(List<string> Allowed) : IScheduledEngagementReviewRule
{
    public List<string> Check(ScheduledEngagementReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementReviewSourceLengthRule(int MaxLength) : IScheduledEngagementReviewRule
{
    public List<string> Check(ScheduledEngagementReview item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled engagement review.</summary>
public sealed record ScheduledEngagementReviewValidator(List<IScheduledEngagementReviewRule> Rules)
{
    public List<string> Validate(ScheduledEngagementReview item)
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

    public void RequireValid(ScheduledEngagementReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledEngagementReview> items)
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

/// <summary>Functions of scheduled engagement review rules.</summary>
public static class ScheduledEngagementReviewRules
{
    public static ScheduledEngagementReviewValidator DefaultScheduledEngagementReviewValidator()
    {
        return new ScheduledEngagementReviewValidator(
            new List<IScheduledEngagementReviewRule>
            {
                new ScheduledEngagementReviewRequiredChannelRule("scheduled engagement review needs a value"),
                new ScheduledEngagementReviewRequiredSourceRule("scheduled engagement review needs an entry"),
                new ScheduledEngagementReviewRequiredCodeRule("scheduled engagement review needs this field"),
                new ScheduledEngagementReviewRequiredLabelRule("scheduled engagement review needs an entry"),
                new ScheduledEngagementReviewAllowedCodeRule(new List<string> { "fallback", "regional" }),
                new ScheduledEngagementReviewSourceLengthRule(32),
            });
    }
}
