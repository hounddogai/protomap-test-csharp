namespace Acme.Workforce.Engagement.Sentiment.Bundle;

/// <summary>A rule a fallback sentiment bundle must pass. Check returns its problems, or none.</summary>
public interface IFallbackSentimentBundleRule
{
    List<string> Check(FallbackSentimentBundle item);
}

public sealed record FallbackSentimentBundleActiveGuardRule(double Limit) : IFallbackSentimentBundleRule
{
    public List<string> Check(FallbackSentimentBundle item)
    {
        if (item.Active && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentBundleRequiredCodeRule(string Message) : IFallbackSentimentBundleRule
{
    public List<string> Check(FallbackSentimentBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentBundleCodeLengthRule(int MaxLength) : IFallbackSentimentBundleRule
{
    public List<string> Check(FallbackSentimentBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback sentiment bundle.</summary>
public sealed record FallbackSentimentBundleValidator(List<IFallbackSentimentBundleRule> Rules)
{
    public List<string> Validate(FallbackSentimentBundle item)
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

    public void RequireValid(FallbackSentimentBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackSentimentBundle> items)
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

/// <summary>Functions of fallback sentiment bundle rules.</summary>
public static class FallbackSentimentBundleRules
{
    public static FallbackSentimentBundleValidator DefaultFallbackSentimentBundleValidator()
    {
        return new FallbackSentimentBundleValidator(
            new List<IFallbackSentimentBundleRule>
            {
                new FallbackSentimentBundleActiveGuardRule(8.6),
                new FallbackSentimentBundleRequiredCodeRule("fallback sentiment bundle needs an entry"),
                new FallbackSentimentBundleCodeLengthRule(3600),
            });
    }
}
