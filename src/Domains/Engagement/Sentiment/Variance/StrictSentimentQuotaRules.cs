namespace Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>A rule a strict sentiment quota must pass. Check returns its problems, or none.</summary>
public interface IStrictSentimentQuotaRule
{
    List<string> Check(StrictSentimentQuota item);
}

public sealed record StrictSentimentQuotaAttemptsRangeRule(int Min, int Max) : IStrictSentimentQuotaRule
{
    public List<string> Check(StrictSentimentQuota item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictSentimentQuotaRequiredLabelRule(string Message) : IStrictSentimentQuotaRule
{
    public List<string> Check(StrictSentimentQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictSentimentQuotaTagsLimitRule(int MaxCount, List<string> Banned) : IStrictSentimentQuotaRule
{
    public List<string> Check(StrictSentimentQuota item)
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

public sealed record StrictSentimentQuotaRequiredCodeRule(string Message) : IStrictSentimentQuotaRule
{
    public List<string> Check(StrictSentimentQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictSentimentQuotaRequiredRegionRule(string Message) : IStrictSentimentQuotaRule
{
    public List<string> Check(StrictSentimentQuota item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictSentimentQuotaCodePrefixRule(string Prefix) : IStrictSentimentQuotaRule
{
    public List<string> Check(StrictSentimentQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict sentiment quota.</summary>
public sealed record StrictSentimentQuotaValidator(List<IStrictSentimentQuotaRule> Rules)
{
    public List<string> Validate(StrictSentimentQuota item)
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

    public void RequireValid(StrictSentimentQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictSentimentQuota> items)
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

/// <summary>Functions of strict sentiment quota rules.</summary>
public static class StrictSentimentQuotaRules
{
    public static StrictSentimentQuotaValidator DefaultStrictSentimentQuotaValidator()
    {
        return new StrictSentimentQuotaValidator(
            new List<IStrictSentimentQuotaRule>
            {
                new StrictSentimentQuotaAttemptsRangeRule(64, 120),
                new StrictSentimentQuotaRequiredLabelRule("strict sentiment quota needs this field"),
                new StrictSentimentQuotaTagsLimitRule(86400, new List<string> { "regional", "legacy" }),
                new StrictSentimentQuotaRequiredCodeRule("strict sentiment quota needs a value"),
                new StrictSentimentQuotaRequiredRegionRule("strict sentiment quota needs this field"),
                new StrictSentimentQuotaCodePrefixRule("REF"),
            });
    }
}
