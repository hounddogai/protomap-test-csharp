namespace Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>A rule a pending sentiment digest must pass. Check returns its problems, or none.</summary>
public interface IPendingSentimentDigestRule
{
    List<string> Check(PendingSentimentDigest item);
}

public sealed record PendingSentimentDigestTagsLimitRule(int MaxCount, List<string> Banned) : IPendingSentimentDigestRule
{
    public List<string> Check(PendingSentimentDigest item)
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

public sealed record PendingSentimentDigestEndsAtRangeRule(int Min, int Max) : IPendingSentimentDigestRule
{
    public List<string> Check(PendingSentimentDigest item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSentimentDigestDescriptionLengthRule(int MaxLength) : IPendingSentimentDigestRule
{
    public List<string> Check(PendingSentimentDigest item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSentimentDigestCodePrefixRule(string Prefix) : IPendingSentimentDigestRule
{
    public List<string> Check(PendingSentimentDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSentimentDigestRequiredCodeRule(string Message) : IPendingSentimentDigestRule
{
    public List<string> Check(PendingSentimentDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingSentimentDigestScoreRangeRule(double Min, double Max) : IPendingSentimentDigestRule
{
    public List<string> Check(PendingSentimentDigest item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending sentiment digest.</summary>
public sealed record PendingSentimentDigestValidator(List<IPendingSentimentDigestRule> Rules)
{
    public List<string> Validate(PendingSentimentDigest item)
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

    public void RequireValid(PendingSentimentDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingSentimentDigest> items)
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

/// <summary>Functions of pending sentiment digest rules.</summary>
public static class PendingSentimentDigestRules
{
    public static PendingSentimentDigestValidator DefaultPendingSentimentDigestValidator()
    {
        return new PendingSentimentDigestValidator(
            new List<IPendingSentimentDigestRule>
            {
                new PendingSentimentDigestTagsLimitRule(3600, new List<string> { "tiered", "manual" }),
                new PendingSentimentDigestEndsAtRangeRule(255, 3600),
                new PendingSentimentDigestDescriptionLengthRule(86400),
                new PendingSentimentDigestCodePrefixRule("X"),
                new PendingSentimentDigestRequiredCodeRule("pending sentiment digest needs this field"),
                new PendingSentimentDigestScoreRangeRule(6.1, 2356.5),
            });
    }
}
