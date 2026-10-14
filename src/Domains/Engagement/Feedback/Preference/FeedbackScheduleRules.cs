namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback schedule must pass. Check returns its problems, or none.</summary>
public interface IFeedbackScheduleRule
{
    List<string> Check(FeedbackSchedule item);
}

public sealed record FeedbackScheduleAllowedCodeRule(List<string> Allowed) : IFeedbackScheduleRule
{
    public List<string> Check(FeedbackSchedule item)
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

public sealed record FeedbackScheduleRequiredOwnerIdRule(string Message) : IFeedbackScheduleRule
{
    public List<string> Check(FeedbackSchedule item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackScheduleOwnerIdLengthRule(int MaxLength) : IFeedbackScheduleRule
{
    public List<string> Check(FeedbackSchedule item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackScheduleRequiredCodeRule(string Message) : IFeedbackScheduleRule
{
    public List<string> Check(FeedbackSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackScheduleLabelLengthRule(int MaxLength) : IFeedbackScheduleRule
{
    public List<string> Check(FeedbackSchedule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback schedule.</summary>
public sealed record FeedbackScheduleValidator(List<IFeedbackScheduleRule> Rules)
{
    public List<string> Validate(FeedbackSchedule item)
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

    public void RequireValid(FeedbackSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackSchedule> items)
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

/// <summary>Functions of feedback schedule rules.</summary>
public static class FeedbackScheduleRules
{
    public static FeedbackScheduleValidator DefaultFeedbackScheduleValidator()
    {
        return new FeedbackScheduleValidator(
            new List<IFeedbackScheduleRule>
            {
                new FeedbackScheduleAllowedCodeRule(new List<string> { "primary" }),
                new FeedbackScheduleRequiredOwnerIdRule("feedback schedule needs a value"),
                new FeedbackScheduleOwnerIdLengthRule(3600),
                new FeedbackScheduleRequiredCodeRule("feedback schedule needs an entry"),
                new FeedbackScheduleLabelLengthRule(80),
            });
    }
}
