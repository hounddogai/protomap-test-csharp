namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>A rule a fallback sentiment preference must pass. Check returns its problems, or none.</summary>
public interface IFallbackSentimentPreferenceRule
{
    List<string> Check(FallbackSentimentPreference item);
}

public sealed record FallbackSentimentPreferenceCodeLengthRule(int MaxLength) : IFallbackSentimentPreferenceRule
{
    public List<string> Check(FallbackSentimentPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentPreferenceCurrencyLengthRule(int MaxLength) : IFallbackSentimentPreferenceRule
{
    public List<string> Check(FallbackSentimentPreference item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentPreferenceActiveGuardRule(double Limit) : IFallbackSentimentPreferenceRule
{
    public List<string> Check(FallbackSentimentPreference item)
    {
        if (item.Active && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentPreferenceRequiredTitleRule(string Message) : IFallbackSentimentPreferenceRule
{
    public List<string> Check(FallbackSentimentPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentPreferenceCodePrefixRule(string Prefix) : IFallbackSentimentPreferenceRule
{
    public List<string> Check(FallbackSentimentPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback sentiment preference.</summary>
public sealed record FallbackSentimentPreferenceValidator(List<IFallbackSentimentPreferenceRule> Rules)
{
    public List<string> Validate(FallbackSentimentPreference item)
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

    public void RequireValid(FallbackSentimentPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackSentimentPreference> items)
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

/// <summary>Functions of fallback sentiment preference rules.</summary>
public static class FallbackSentimentPreferenceRules
{
    public static FallbackSentimentPreferenceValidator DefaultFallbackSentimentPreferenceValidator()
    {
        return new FallbackSentimentPreferenceValidator(
            new List<IFallbackSentimentPreferenceRule>
            {
                new FallbackSentimentPreferenceCodeLengthRule(16),
                new FallbackSentimentPreferenceCurrencyLengthRule(255),
                new FallbackSentimentPreferenceActiveGuardRule(4.7),
                new FallbackSentimentPreferenceRequiredTitleRule("fallback sentiment preference needs this field"),
                new FallbackSentimentPreferenceCodePrefixRule("X"),
            });
    }
}
