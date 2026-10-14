namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment batch must pass. Check returns its problems, or none.</summary>
public interface ISentimentBatchRule
{
    List<string> Check(SentimentBatch item);
}

public sealed record SentimentBatchRequiredCodeRule(string Message) : ISentimentBatchRule
{
    public List<string> Check(SentimentBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentBatchTitleLengthRule(int MaxLength) : ISentimentBatchRule
{
    public List<string> Check(SentimentBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentBatchAllowedSourceRule(List<string> Allowed) : ISentimentBatchRule
{
    public List<string> Check(SentimentBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment batch.</summary>
public sealed record SentimentBatchValidator(List<ISentimentBatchRule> Rules)
{
    public List<string> Validate(SentimentBatch item)
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

    public void RequireValid(SentimentBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentBatch> items)
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

/// <summary>Functions of sentiment batch rules.</summary>
public static class SentimentBatchRules
{
    public static SentimentBatchValidator DefaultSentimentBatchValidator()
    {
        return new SentimentBatchValidator(
            new List<ISentimentBatchRule>
            {
                new SentimentBatchRequiredCodeRule("sentiment batch needs this field"),
                new SentimentBatchTitleLengthRule(3600),
                new SentimentBatchAllowedSourceRule(new List<string> { "batched", "batched", "rolling" }),
            });
    }
}
