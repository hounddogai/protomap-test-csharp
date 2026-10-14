namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched sentiment manifest must pass. Check returns its problems, or none.</summary>
public interface IBatchedSentimentManifestRule
{
    List<string> Check(BatchedSentimentManifest item);
}

public sealed record BatchedSentimentManifestRequiredCodeRule(string Message) : IBatchedSentimentManifestRule
{
    public List<string> Check(BatchedSentimentManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedSentimentManifestEndsAtRangeRule(int Min, int Max) : IBatchedSentimentManifestRule
{
    public List<string> Check(BatchedSentimentManifest item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedSentimentManifestRequiredNameRule(string Message) : IBatchedSentimentManifestRule
{
    public List<string> Check(BatchedSentimentManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedSentimentManifestCodePrefixRule(string Prefix) : IBatchedSentimentManifestRule
{
    public List<string> Check(BatchedSentimentManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedSentimentManifestNameLengthRule(int MaxLength) : IBatchedSentimentManifestRule
{
    public List<string> Check(BatchedSentimentManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedSentimentManifestAllowedNameRule(List<string> Allowed) : IBatchedSentimentManifestRule
{
    public List<string> Check(BatchedSentimentManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched sentiment manifest.</summary>
public sealed record BatchedSentimentManifestValidator(List<IBatchedSentimentManifestRule> Rules)
{
    public List<string> Validate(BatchedSentimentManifest item)
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

    public void RequireValid(BatchedSentimentManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedSentimentManifest> items)
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

/// <summary>Functions of batched sentiment manifest rules.</summary>
public static class BatchedSentimentManifestRules
{
    public static BatchedSentimentManifestValidator DefaultBatchedSentimentManifestValidator()
    {
        return new BatchedSentimentManifestValidator(
            new List<IBatchedSentimentManifestRule>
            {
                new BatchedSentimentManifestRequiredCodeRule("batched sentiment manifest needs an entry"),
                new BatchedSentimentManifestEndsAtRangeRule(32, 1000),
                new BatchedSentimentManifestRequiredNameRule("batched sentiment manifest needs an entry"),
                new BatchedSentimentManifestCodePrefixRule("Q"),
                new BatchedSentimentManifestNameLengthRule(32),
                new BatchedSentimentManifestAllowedNameRule(new List<string>()),
            });
    }
}
