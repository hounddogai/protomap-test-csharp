namespace Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>A rule a engagement notice must pass. Check returns its problems, or none.</summary>
public interface IEngagementNoticeRule
{
    List<string> Check(EngagementNotice item);
}

public sealed record EngagementNoticeReviewerLengthRule(int MaxLength) : IEngagementNoticeRule
{
    public List<string> Check(EngagementNotice item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementNoticeLabelLengthRule(int MaxLength) : IEngagementNoticeRule
{
    public List<string> Check(EngagementNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementNoticeRequiredReviewerRule(string Message) : IEngagementNoticeRule
{
    public List<string> Check(EngagementNotice item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement notice.</summary>
public sealed record EngagementNoticeValidator(List<IEngagementNoticeRule> Rules)
{
    public List<string> Validate(EngagementNotice item)
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

    public void RequireValid(EngagementNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementNotice> items)
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

/// <summary>Functions of engagement notice rules.</summary>
public static class EngagementNoticeRules
{
    public static EngagementNoticeValidator DefaultEngagementNoticeValidator()
    {
        return new EngagementNoticeValidator(
            new List<IEngagementNoticeRule>
            {
                new EngagementNoticeReviewerLengthRule(120),
                new EngagementNoticeLabelLengthRule(32),
                new EngagementNoticeRequiredReviewerRule("engagement notice needs an entry"),
            });
    }
}
