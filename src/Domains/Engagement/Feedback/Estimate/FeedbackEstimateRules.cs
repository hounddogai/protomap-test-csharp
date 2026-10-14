namespace Acme.Workforce.Engagement.Feedback.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback estimate must pass. Check returns its problems, or none.</summary>
public interface IFeedbackEstimateRule
{
    List<string> Check(FeedbackEstimate item);
}

public sealed record FeedbackEstimateRequiredCurrencyRule(string Message) : IFeedbackEstimateRule
{
    public List<string> Check(FeedbackEstimate item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackEstimatePinnedGuardRule(int Limit) : IFeedbackEstimateRule
{
    public List<string> Check(FeedbackEstimate item)
    {
        if (item.Pinned && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackEstimateCodeLengthRule(int MaxLength) : IFeedbackEstimateRule
{
    public List<string> Check(FeedbackEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackEstimateTitleLengthRule(int MaxLength) : IFeedbackEstimateRule
{
    public List<string> Check(FeedbackEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackEstimateCurrencyLengthRule(int MaxLength) : IFeedbackEstimateRule
{
    public List<string> Check(FeedbackEstimate item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackEstimateAllowedCurrencyRule(List<string> Allowed) : IFeedbackEstimateRule
{
    public List<string> Check(FeedbackEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback estimate.</summary>
public sealed record FeedbackEstimateValidator(List<IFeedbackEstimateRule> Rules)
{
    public List<string> Validate(FeedbackEstimate item)
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

    public void RequireValid(FeedbackEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackEstimate> items)
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

/// <summary>Functions of feedback estimate rules.</summary>
public static class FeedbackEstimateRules
{
    public static FeedbackEstimateValidator DefaultFeedbackEstimateValidator()
    {
        return new FeedbackEstimateValidator(
            new List<IFeedbackEstimateRule>
            {
                new FeedbackEstimateRequiredCurrencyRule("feedback estimate needs an entry"),
                new FeedbackEstimatePinnedGuardRule(80),
                new FeedbackEstimateCodeLengthRule(32),
                new FeedbackEstimateTitleLengthRule(255),
                new FeedbackEstimateCurrencyLengthRule(64),
                new FeedbackEstimateAllowedCurrencyRule(new List<string> { "legacy" }),
            });
    }
}
