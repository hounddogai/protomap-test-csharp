namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending sentiment batch must pass. Check returns its problems, or none.</summary>
public interface IPendingSentimentBatchRule
{
    List<string> Check(PendingSentimentBatch item);
}

public sealed record PendingSentimentBatchRequiredCodeRule(string Message) : IPendingSentimentBatchRule
{
    public List<string> Check(PendingSentimentBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingSentimentBatchRequiredReviewerRule(string Message) : IPendingSentimentBatchRule
{
    public List<string> Check(PendingSentimentBatch item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingSentimentBatchRequiredNameRule(string Message) : IPendingSentimentBatchRule
{
    public List<string> Check(PendingSentimentBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingSentimentBatchAllowedCodeRule(List<string> Allowed) : IPendingSentimentBatchRule
{
    public List<string> Check(PendingSentimentBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PendingSentimentBatchReviewerLengthRule(int MaxLength) : IPendingSentimentBatchRule
{
    public List<string> Check(PendingSentimentBatch item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSentimentBatchCodePrefixRule(string Prefix) : IPendingSentimentBatchRule
{
    public List<string> Check(PendingSentimentBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending sentiment batch.</summary>
public sealed record PendingSentimentBatchValidator(List<IPendingSentimentBatchRule> Rules)
{
    public List<string> Validate(PendingSentimentBatch item)
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

    public void RequireValid(PendingSentimentBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingSentimentBatch> items)
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

/// <summary>Functions of pending sentiment batch rules.</summary>
public static class PendingSentimentBatchRules
{
    public static PendingSentimentBatchValidator DefaultPendingSentimentBatchValidator()
    {
        return new PendingSentimentBatchValidator(
            new List<IPendingSentimentBatchRule>
            {
                new PendingSentimentBatchRequiredCodeRule("pending sentiment batch needs this field"),
                new PendingSentimentBatchRequiredReviewerRule("pending sentiment batch needs this field"),
                new PendingSentimentBatchRequiredNameRule("pending sentiment batch needs this field"),
                new PendingSentimentBatchAllowedCodeRule(new List<string> { "strict", "regional", "seasonal" }),
                new PendingSentimentBatchReviewerLengthRule(80),
                new PendingSentimentBatchCodePrefixRule("Q"),
            });
    }
}
