namespace Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>A rule a batched feedback notice must pass. Check returns its problems, or none.</summary>
public interface IBatchedFeedbackNoticeRule
{
    List<string> Check(BatchedFeedbackNotice item);
}

public sealed record BatchedFeedbackNoticeCodePrefixRule(string Prefix) : IBatchedFeedbackNoticeRule
{
    public List<string> Check(BatchedFeedbackNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackNoticeRequiredTitleRule(string Message) : IBatchedFeedbackNoticeRule
{
    public List<string> Check(BatchedFeedbackNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackNoticeRequiredLabelRule(string Message) : IBatchedFeedbackNoticeRule
{
    public List<string> Check(BatchedFeedbackNotice item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackNoticeSkillLengthRule(int MaxLength) : IBatchedFeedbackNoticeRule
{
    public List<string> Check(BatchedFeedbackNotice item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackNoticeRequiredSkillRule(string Message) : IBatchedFeedbackNoticeRule
{
    public List<string> Check(BatchedFeedbackNotice item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackNoticeLabelLengthRule(int MaxLength) : IBatchedFeedbackNoticeRule
{
    public List<string> Check(BatchedFeedbackNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched feedback notice.</summary>
public sealed record BatchedFeedbackNoticeValidator(List<IBatchedFeedbackNoticeRule> Rules)
{
    public List<string> Validate(BatchedFeedbackNotice item)
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

    public void RequireValid(BatchedFeedbackNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedFeedbackNotice> items)
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

/// <summary>Functions of batched feedback notice rules.</summary>
public static class BatchedFeedbackNoticeRules
{
    public static BatchedFeedbackNoticeValidator DefaultBatchedFeedbackNoticeValidator()
    {
        return new BatchedFeedbackNoticeValidator(
            new List<IBatchedFeedbackNoticeRule>
            {
                new BatchedFeedbackNoticeCodePrefixRule("OPS"),
                new BatchedFeedbackNoticeRequiredTitleRule("batched feedback notice needs this field"),
                new BatchedFeedbackNoticeRequiredLabelRule("batched feedback notice needs an entry"),
                new BatchedFeedbackNoticeSkillLengthRule(16),
                new BatchedFeedbackNoticeRequiredSkillRule("batched feedback notice needs an entry"),
                new BatchedFeedbackNoticeLabelLengthRule(32),
            });
    }
}
