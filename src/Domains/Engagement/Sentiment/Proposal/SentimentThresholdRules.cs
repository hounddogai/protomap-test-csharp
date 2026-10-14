namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment threshold must pass. Check returns its problems, or none.</summary>
public interface ISentimentThresholdRule
{
    List<string> Check(SentimentThreshold item);
}

public sealed record SentimentThresholdAllowedTitleRule(List<string> Allowed) : ISentimentThresholdRule
{
    public List<string> Check(SentimentThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record SentimentThresholdCreatedAtRangeRule(int Min, int Max) : ISentimentThresholdRule
{
    public List<string> Check(SentimentThreshold item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentThresholdCodePrefixRule(string Prefix) : ISentimentThresholdRule
{
    public List<string> Check(SentimentThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentThresholdCodeLengthRule(int MaxLength) : ISentimentThresholdRule
{
    public List<string> Check(SentimentThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentThresholdLabelLengthRule(int MaxLength) : ISentimentThresholdRule
{
    public List<string> Check(SentimentThreshold item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentThresholdRequiredCodeRule(string Message) : ISentimentThresholdRule
{
    public List<string> Check(SentimentThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment threshold.</summary>
public sealed record SentimentThresholdValidator(List<ISentimentThresholdRule> Rules)
{
    public List<string> Validate(SentimentThreshold item)
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

    public void RequireValid(SentimentThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentThreshold> items)
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

/// <summary>Functions of sentiment threshold rules.</summary>
public static class SentimentThresholdRules
{
    public static SentimentThresholdValidator DefaultSentimentThresholdValidator()
    {
        return new SentimentThresholdValidator(
            new List<ISentimentThresholdRule>
            {
                new SentimentThresholdAllowedTitleRule(new List<string> { "global", "lenient", "seasonal" }),
                new SentimentThresholdCreatedAtRangeRule(80, 86400),
                new SentimentThresholdCodePrefixRule("REF"),
                new SentimentThresholdCodeLengthRule(64),
                new SentimentThresholdLabelLengthRule(32),
                new SentimentThresholdRequiredCodeRule("sentiment threshold needs this field"),
            });
    }
}
