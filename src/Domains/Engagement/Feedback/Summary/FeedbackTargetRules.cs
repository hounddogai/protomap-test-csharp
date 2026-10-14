namespace Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>A rule a feedback target must pass. Check returns its problems, or none.</summary>
public interface IFeedbackTargetRule
{
    List<string> Check(FeedbackTarget item);
}

public sealed record FeedbackTargetRequiredCodeRule(string Message) : IFeedbackTargetRule
{
    public List<string> Check(FeedbackTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackTargetSummaryLengthRule(int MaxLength) : IFeedbackTargetRule
{
    public List<string> Check(FeedbackTarget item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackTargetCountryLengthRule(int MaxLength) : IFeedbackTargetRule
{
    public List<string> Check(FeedbackTarget item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackTargetRequiredSummaryRule(string Message) : IFeedbackTargetRule
{
    public List<string> Check(FeedbackTarget item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackTargetRequiredCountryRule(string Message) : IFeedbackTargetRule
{
    public List<string> Check(FeedbackTarget item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackTargetRequiredTitleRule(string Message) : IFeedbackTargetRule
{
    public List<string> Check(FeedbackTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback target.</summary>
public sealed record FeedbackTargetValidator(List<IFeedbackTargetRule> Rules)
{
    public List<string> Validate(FeedbackTarget item)
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

    public void RequireValid(FeedbackTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackTarget> items)
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

/// <summary>Functions of feedback target rules.</summary>
public static class FeedbackTargetRules
{
    public static FeedbackTargetValidator DefaultFeedbackTargetValidator()
    {
        return new FeedbackTargetValidator(
            new List<IFeedbackTargetRule>
            {
                new FeedbackTargetRequiredCodeRule("feedback target needs a value"),
                new FeedbackTargetSummaryLengthRule(16),
                new FeedbackTargetCountryLengthRule(16),
                new FeedbackTargetRequiredSummaryRule("feedback target needs an entry"),
                new FeedbackTargetRequiredCountryRule("feedback target needs a value"),
                new FeedbackTargetRequiredTitleRule("feedback target needs this field"),
            });
    }
}
