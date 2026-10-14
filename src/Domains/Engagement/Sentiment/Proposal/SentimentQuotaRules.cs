namespace Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>A rule a sentiment quota must pass. Check returns its problems, or none.</summary>
public interface ISentimentQuotaRule
{
    List<string> Check(SentimentQuota item);
}

public sealed record SentimentQuotaQuantityRangeRule(int Min, int Max) : ISentimentQuotaRule
{
    public List<string> Check(SentimentQuota item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentQuotaNameLengthRule(int MaxLength) : ISentimentQuotaRule
{
    public List<string> Check(SentimentQuota item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentQuotaCodePrefixRule(string Prefix) : ISentimentQuotaRule
{
    public List<string> Check(SentimentQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment quota.</summary>
public sealed record SentimentQuotaValidator(List<ISentimentQuotaRule> Rules)
{
    public List<string> Validate(SentimentQuota item)
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

    public void RequireValid(SentimentQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentQuota> items)
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

/// <summary>Functions of sentiment quota rules.</summary>
public static class SentimentQuotaRules
{
    public static SentimentQuotaValidator DefaultSentimentQuotaValidator()
    {
        return new SentimentQuotaValidator(
            new List<ISentimentQuotaRule>
            {
                new SentimentQuotaQuantityRangeRule(80, 3600),
                new SentimentQuotaNameLengthRule(64),
                new SentimentQuotaCodePrefixRule("OPS"),
            });
    }
}
