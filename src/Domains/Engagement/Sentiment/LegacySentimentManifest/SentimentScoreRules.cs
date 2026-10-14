namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>A rule a sentiment score must pass. Check returns its problems, or none.</summary>
public interface ISentimentScoreRule
{
    List<string> Check(SentimentScore item);
}

public sealed record SentimentScoreRequiredCodeRule(string Message) : ISentimentScoreRule
{
    public List<string> Check(SentimentScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentScoreRequiredBadgeRule(string Message) : ISentimentScoreRule
{
    public List<string> Check(SentimentScore item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentScoreNameLengthRule(int MaxLength) : ISentimentScoreRule
{
    public List<string> Check(SentimentScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentScoreCodePrefixRule(string Prefix) : ISentimentScoreRule
{
    public List<string> Check(SentimentScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentScoreRatioRangeRule(double Min, double Max) : ISentimentScoreRule
{
    public List<string> Check(SentimentScore item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentScoreBadgeLengthRule(int MaxLength) : ISentimentScoreRule
{
    public List<string> Check(SentimentScore item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment score.</summary>
public sealed record SentimentScoreValidator(List<ISentimentScoreRule> Rules)
{
    public List<string> Validate(SentimentScore item)
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

    public void RequireValid(SentimentScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentScore> items)
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

/// <summary>Functions of sentiment score rules.</summary>
public static class SentimentScoreRules
{
    public static SentimentScoreValidator DefaultSentimentScoreValidator()
    {
        return new SentimentScoreValidator(
            new List<ISentimentScoreRule>
            {
                new SentimentScoreRequiredCodeRule("sentiment score needs this field"),
                new SentimentScoreRequiredBadgeRule("sentiment score needs a value"),
                new SentimentScoreNameLengthRule(80),
                new SentimentScoreCodePrefixRule("OPS"),
                new SentimentScoreRatioRangeRule(8.3, 4262.7),
                new SentimentScoreBadgeLengthRule(3600),
            });
    }
}
