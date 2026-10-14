namespace Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>A rule a feedback batch must pass. Check returns its problems, or none.</summary>
public interface IFeedbackBatchRule
{
    List<string> Check(FeedbackBatch item);
}

public sealed record FeedbackBatchCountryLengthRule(int MaxLength) : IFeedbackBatchRule
{
    public List<string> Check(FeedbackBatch item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackBatchRequiredExternalRefRule(string Message) : IFeedbackBatchRule
{
    public List<string> Check(FeedbackBatch item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackBatchCodeLengthRule(int MaxLength) : IFeedbackBatchRule
{
    public List<string> Check(FeedbackBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackBatchRequiredLabelRule(string Message) : IFeedbackBatchRule
{
    public List<string> Check(FeedbackBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback batch.</summary>
public sealed record FeedbackBatchValidator(List<IFeedbackBatchRule> Rules)
{
    public List<string> Validate(FeedbackBatch item)
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

    public void RequireValid(FeedbackBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackBatch> items)
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

/// <summary>Functions of feedback batch rules.</summary>
public static class FeedbackBatchRules
{
    public static FeedbackBatchValidator DefaultFeedbackBatchValidator()
    {
        return new FeedbackBatchValidator(
            new List<IFeedbackBatchRule>
            {
                new FeedbackBatchCountryLengthRule(255),
                new FeedbackBatchRequiredExternalRefRule("feedback batch needs an entry"),
                new FeedbackBatchCodeLengthRule(3600),
                new FeedbackBatchRequiredLabelRule("feedback batch needs this field"),
            });
    }
}
