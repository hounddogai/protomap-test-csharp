namespace Acme.Workforce.Engagement.Feedback.RecordItem;

/// <summary>A rule a feedback score must pass. Check returns its problems, or none.</summary>
public interface IFeedbackScoreRule
{
    List<string> Check(FeedbackScore item);
}

public sealed record FeedbackScoreRequiredSummaryRule(string Message) : IFeedbackScoreRule
{
    public List<string> Check(FeedbackScore item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackScoreSummaryLengthRule(int MaxLength) : IFeedbackScoreRule
{
    public List<string> Check(FeedbackScore item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackScoreExpiresAtRangeRule(int Min, int Max) : IFeedbackScoreRule
{
    public List<string> Check(FeedbackScore item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackScoreRequiredCodeRule(string Message) : IFeedbackScoreRule
{
    public List<string> Check(FeedbackScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackScoreNameLengthRule(int MaxLength) : IFeedbackScoreRule
{
    public List<string> Check(FeedbackScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback score.</summary>
public sealed record FeedbackScoreValidator(List<IFeedbackScoreRule> Rules)
{
    public List<string> Validate(FeedbackScore item)
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

    public void RequireValid(FeedbackScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackScore> items)
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

/// <summary>Functions of feedback score rules.</summary>
public static class FeedbackScoreRules
{
    public static FeedbackScoreValidator DefaultFeedbackScoreValidator()
    {
        return new FeedbackScoreValidator(
            new List<IFeedbackScoreRule>
            {
                new FeedbackScoreRequiredSummaryRule("feedback score needs a value"),
                new FeedbackScoreSummaryLengthRule(80),
                new FeedbackScoreExpiresAtRangeRule(16, 16),
                new FeedbackScoreRequiredCodeRule("feedback score needs an entry"),
                new FeedbackScoreNameLengthRule(1000),
            });
    }
}
