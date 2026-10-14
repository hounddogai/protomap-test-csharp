namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback notice must pass. Check returns its problems, or none.</summary>
public interface IFeedbackNoticeRule
{
    List<string> Check(FeedbackNotice item);
}

public sealed record FeedbackNoticeLabelLengthRule(int MaxLength) : IFeedbackNoticeRule
{
    public List<string> Check(FeedbackNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackNoticeAllowedLabelRule(List<string> Allowed) : IFeedbackNoticeRule
{
    public List<string> Check(FeedbackNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackNoticeRevisionRangeRule(int Min, int Max) : IFeedbackNoticeRule
{
    public List<string> Check(FeedbackNotice item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback notice.</summary>
public sealed record FeedbackNoticeValidator(List<IFeedbackNoticeRule> Rules)
{
    public List<string> Validate(FeedbackNotice item)
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

    public void RequireValid(FeedbackNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackNotice> items)
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

/// <summary>Functions of feedback notice rules.</summary>
public static class FeedbackNoticeRules
{
    public static FeedbackNoticeValidator DefaultFeedbackNoticeValidator()
    {
        return new FeedbackNoticeValidator(
            new List<IFeedbackNoticeRule>
            {
                new FeedbackNoticeLabelLengthRule(3600),
                new FeedbackNoticeAllowedLabelRule(new List<string> { "manual" }),
                new FeedbackNoticeRevisionRangeRule(255, 1000),
            });
    }
}
