namespace Acme.Workforce.Engagement.Sentiment.Baseline;

/// <summary>A rule a sentiment segment must pass. Check returns its problems, or none.</summary>
public interface ISentimentSegmentRule
{
    List<string> Check(SentimentSegment item);
}

public sealed record SentimentSegmentRequiredNameRule(string Message) : ISentimentSegmentRule
{
    public List<string> Check(SentimentSegment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentSegmentCodeLengthRule(int MaxLength) : ISentimentSegmentRule
{
    public List<string> Check(SentimentSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentSegmentCodePrefixRule(string Prefix) : ISentimentSegmentRule
{
    public List<string> Check(SentimentSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment segment.</summary>
public sealed record SentimentSegmentValidator(List<ISentimentSegmentRule> Rules)
{
    public List<string> Validate(SentimentSegment item)
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

    public void RequireValid(SentimentSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentSegment> items)
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

/// <summary>Functions of sentiment segment rules.</summary>
public static class SentimentSegmentRules
{
    public static SentimentSegmentValidator DefaultSentimentSegmentValidator()
    {
        return new SentimentSegmentValidator(
            new List<ISentimentSegmentRule>
            {
                new SentimentSegmentRequiredNameRule("sentiment segment needs an entry"),
                new SentimentSegmentCodeLengthRule(1000),
                new SentimentSegmentCodePrefixRule("OPS"),
            });
    }
}
