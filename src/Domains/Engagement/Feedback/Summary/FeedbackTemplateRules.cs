namespace Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>A rule a feedback template must pass. Check returns its problems, or none.</summary>
public interface IFeedbackTemplateRule
{
    List<string> Check(FeedbackTemplate item);
}

public sealed record FeedbackTemplateAmountRangeRule(double Min, double Max) : IFeedbackTemplateRule
{
    public List<string> Check(FeedbackTemplate item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FeedbackTemplateCodePrefixRule(string Prefix) : IFeedbackTemplateRule
{
    public List<string> Check(FeedbackTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackTemplateRequiredSummaryRule(string Message) : IFeedbackTemplateRule
{
    public List<string> Check(FeedbackTemplate item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackTemplateNotesLengthRule(int MaxLength) : IFeedbackTemplateRule
{
    public List<string> Check(FeedbackTemplate item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackTemplateTagsLimitRule(int MaxCount, List<string> Banned) : IFeedbackTemplateRule
{
    public List<string> Check(FeedbackTemplate item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record FeedbackTemplateCreatedAtRangeRule(int Min, int Max) : IFeedbackTemplateRule
{
    public List<string> Check(FeedbackTemplate item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a feedback template.</summary>
public sealed record FeedbackTemplateValidator(List<IFeedbackTemplateRule> Rules)
{
    public List<string> Validate(FeedbackTemplate item)
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

    public void RequireValid(FeedbackTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackTemplate> items)
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

/// <summary>Functions of feedback template rules.</summary>
public static class FeedbackTemplateRules
{
    public static FeedbackTemplateValidator DefaultFeedbackTemplateValidator()
    {
        return new FeedbackTemplateValidator(
            new List<IFeedbackTemplateRule>
            {
                new FeedbackTemplateAmountRangeRule(1.7, 3923.0),
                new FeedbackTemplateCodePrefixRule("REF"),
                new FeedbackTemplateRequiredSummaryRule("feedback template needs a value"),
                new FeedbackTemplateNotesLengthRule(120),
                new FeedbackTemplateTagsLimitRule(64, new List<string> { "seasonal" }),
                new FeedbackTemplateCreatedAtRangeRule(64, 120),
            });
    }
}
