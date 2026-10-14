namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback audit must pass. Check returns its problems, or none.</summary>
public interface IFeedbackAuditRule
{
    List<string> Check(FeedbackAudit item);
}

public sealed record FeedbackAuditNameLengthRule(int MaxLength) : IFeedbackAuditRule
{
    public List<string> Check(FeedbackAudit item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackAuditTitleLengthRule(int MaxLength) : IFeedbackAuditRule
{
    public List<string> Check(FeedbackAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackAuditRequiredTitleRule(string Message) : IFeedbackAuditRule
{
    public List<string> Check(FeedbackAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackAuditAllowedRegionRule(List<string> Allowed) : IFeedbackAuditRule
{
    public List<string> Check(FeedbackAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackAuditMinimumRangeRule(double Min, double Max) : IFeedbackAuditRule
{
    public List<string> Check(FeedbackAudit item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackAuditCodeLengthRule(int MaxLength) : IFeedbackAuditRule
{
    public List<string> Check(FeedbackAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback audit.</summary>
public sealed record FeedbackAuditValidator(List<IFeedbackAuditRule> Rules)
{
    public List<string> Validate(FeedbackAudit item)
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

    public void RequireValid(FeedbackAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackAudit> items)
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

/// <summary>Functions of feedback audit rules.</summary>
public static class FeedbackAuditRules
{
    public static FeedbackAuditValidator DefaultFeedbackAuditValidator()
    {
        return new FeedbackAuditValidator(
            new List<IFeedbackAuditRule>
            {
                new FeedbackAuditNameLengthRule(16),
                new FeedbackAuditTitleLengthRule(86400),
                new FeedbackAuditRequiredTitleRule("feedback audit needs an entry"),
                new FeedbackAuditAllowedRegionRule(new List<string> { "weighted" }),
                new FeedbackAuditMinimumRangeRule(4.2, 430.9),
                new FeedbackAuditCodeLengthRule(32),
            });
    }
}
