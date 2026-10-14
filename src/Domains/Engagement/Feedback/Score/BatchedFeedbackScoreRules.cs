namespace Acme.Workforce.Engagement.Feedback.Score;

/// <summary>A rule a batched feedback score must pass. Check returns its problems, or none.</summary>
public interface IBatchedFeedbackScoreRule
{
    List<string> Check(BatchedFeedbackScore item);
}

public sealed record BatchedFeedbackScoreRequiredTitleRule(string Message) : IBatchedFeedbackScoreRule
{
    public List<string> Check(BatchedFeedbackScore item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackScoreRequiredCurrencyRule(string Message) : IBatchedFeedbackScoreRule
{
    public List<string> Check(BatchedFeedbackScore item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackScoreCurrencyLengthRule(int MaxLength) : IBatchedFeedbackScoreRule
{
    public List<string> Check(BatchedFeedbackScore item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackScoreRequiredReviewerRule(string Message) : IBatchedFeedbackScoreRule
{
    public List<string> Check(BatchedFeedbackScore item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackScoreRequiredCodeRule(string Message) : IBatchedFeedbackScoreRule
{
    public List<string> Check(BatchedFeedbackScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched feedback score.</summary>
public sealed record BatchedFeedbackScoreValidator(List<IBatchedFeedbackScoreRule> Rules)
{
    public List<string> Validate(BatchedFeedbackScore item)
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

    public void RequireValid(BatchedFeedbackScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedFeedbackScore> items)
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

/// <summary>Functions of batched feedback score rules.</summary>
public static class BatchedFeedbackScoreRules
{
    public static BatchedFeedbackScoreValidator DefaultBatchedFeedbackScoreValidator()
    {
        return new BatchedFeedbackScoreValidator(
            new List<IBatchedFeedbackScoreRule>
            {
                new BatchedFeedbackScoreRequiredTitleRule("batched feedback score needs an entry"),
                new BatchedFeedbackScoreRequiredCurrencyRule("batched feedback score needs a value"),
                new BatchedFeedbackScoreCurrencyLengthRule(64),
                new BatchedFeedbackScoreRequiredReviewerRule("batched feedback score needs a value"),
                new BatchedFeedbackScoreRequiredCodeRule("batched feedback score needs a value"),
            });
    }
}
