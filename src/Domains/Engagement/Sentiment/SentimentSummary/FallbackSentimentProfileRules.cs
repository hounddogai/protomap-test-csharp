namespace Acme.Workforce.Engagement.Sentiment.SentimentSummary;

/// <summary>A rule a fallback sentiment profile must pass. Check returns its problems, or none.</summary>
public interface IFallbackSentimentProfileRule
{
    List<string> Check(FallbackSentimentProfile item);
}

public sealed record FallbackSentimentProfileTitleLengthRule(int MaxLength) : IFallbackSentimentProfileRule
{
    public List<string> Check(FallbackSentimentProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentProfileRequiredTitleRule(string Message) : IFallbackSentimentProfileRule
{
    public List<string> Check(FallbackSentimentProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentProfileAttemptsRangeRule(int Min, int Max) : IFallbackSentimentProfileRule
{
    public List<string> Check(FallbackSentimentProfile item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentProfileRequiredCodeRule(string Message) : IFallbackSentimentProfileRule
{
    public List<string> Check(FallbackSentimentProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback sentiment profile.</summary>
public sealed record FallbackSentimentProfileValidator(List<IFallbackSentimentProfileRule> Rules)
{
    public List<string> Validate(FallbackSentimentProfile item)
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

    public void RequireValid(FallbackSentimentProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackSentimentProfile> items)
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

/// <summary>Functions of fallback sentiment profile rules.</summary>
public static class FallbackSentimentProfileRules
{
    public static FallbackSentimentProfileValidator DefaultFallbackSentimentProfileValidator()
    {
        return new FallbackSentimentProfileValidator(
            new List<IFallbackSentimentProfileRule>
            {
                new FallbackSentimentProfileTitleLengthRule(120),
                new FallbackSentimentProfileRequiredTitleRule("fallback sentiment profile needs a value"),
                new FallbackSentimentProfileAttemptsRangeRule(64, 86400),
                new FallbackSentimentProfileRequiredCodeRule("fallback sentiment profile needs an entry"),
            });
    }
}
