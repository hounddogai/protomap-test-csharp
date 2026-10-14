namespace Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>A rule a feedback policy must pass. Check returns its problems, or none.</summary>
public interface IFeedbackPolicyRule
{
    List<string> Check(FeedbackPolicy item);
}

public sealed record FeedbackPolicyRequiredTitleRule(string Message) : IFeedbackPolicyRule
{
    public List<string> Check(FeedbackPolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackPolicyCodePrefixRule(string Prefix) : IFeedbackPolicyRule
{
    public List<string> Check(FeedbackPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackPolicyLocaleLengthRule(int MaxLength) : IFeedbackPolicyRule
{
    public List<string> Check(FeedbackPolicy item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback policy.</summary>
public sealed record FeedbackPolicyValidator(List<IFeedbackPolicyRule> Rules)
{
    public List<string> Validate(FeedbackPolicy item)
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

    public void RequireValid(FeedbackPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackPolicy> items)
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

/// <summary>Functions of feedback policy rules.</summary>
public static class FeedbackPolicyRules
{
    public static FeedbackPolicyValidator DefaultFeedbackPolicyValidator()
    {
        return new FeedbackPolicyValidator(
            new List<IFeedbackPolicyRule>
            {
                new FeedbackPolicyRequiredTitleRule("feedback policy needs this field"),
                new FeedbackPolicyCodePrefixRule("ACM"),
                new FeedbackPolicyLocaleLengthRule(3600),
            });
    }
}
