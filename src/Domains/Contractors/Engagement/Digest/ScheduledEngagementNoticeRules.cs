namespace Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>A rule a scheduled engagement notice must pass. Check returns its problems, or none.</summary>
public interface IScheduledEngagementNoticeRule
{
    List<string> Check(ScheduledEngagementNotice item);
}

public sealed record ScheduledEngagementNoticeRequiredLabelRule(string Message) : IScheduledEngagementNoticeRule
{
    public List<string> Check(ScheduledEngagementNotice item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementNoticeRequiredCodeRule(string Message) : IScheduledEngagementNoticeRule
{
    public List<string> Check(ScheduledEngagementNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementNoticeLabelLengthRule(int MaxLength) : IScheduledEngagementNoticeRule
{
    public List<string> Check(ScheduledEngagementNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementNoticeCodeLengthRule(int MaxLength) : IScheduledEngagementNoticeRule
{
    public List<string> Check(ScheduledEngagementNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementNoticeNameLengthRule(int MaxLength) : IScheduledEngagementNoticeRule
{
    public List<string> Check(ScheduledEngagementNotice item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled engagement notice.</summary>
public sealed record ScheduledEngagementNoticeValidator(List<IScheduledEngagementNoticeRule> Rules)
{
    public List<string> Validate(ScheduledEngagementNotice item)
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

    public void RequireValid(ScheduledEngagementNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledEngagementNotice> items)
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

/// <summary>Functions of scheduled engagement notice rules.</summary>
public static class ScheduledEngagementNoticeRules
{
    public static ScheduledEngagementNoticeValidator DefaultScheduledEngagementNoticeValidator()
    {
        return new ScheduledEngagementNoticeValidator(
            new List<IScheduledEngagementNoticeRule>
            {
                new ScheduledEngagementNoticeRequiredLabelRule("scheduled engagement notice needs this field"),
                new ScheduledEngagementNoticeRequiredCodeRule("scheduled engagement notice needs this field"),
                new ScheduledEngagementNoticeLabelLengthRule(255),
                new ScheduledEngagementNoticeCodeLengthRule(80),
                new ScheduledEngagementNoticeNameLengthRule(32),
            });
    }
}
