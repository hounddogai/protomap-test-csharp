namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>A rule a sentiment preference must pass. Check returns its problems, or none.</summary>
public interface ISentimentPreferenceRule
{
    List<string> Check(SentimentPreference item);
}

public sealed record SentimentPreferenceCodePrefixRule(string Prefix) : ISentimentPreferenceRule
{
    public List<string> Check(SentimentPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentPreferenceRequiredTitleRule(string Message) : ISentimentPreferenceRule
{
    public List<string> Check(SentimentPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentPreferenceExternalRefLengthRule(int MaxLength) : ISentimentPreferenceRule
{
    public List<string> Check(SentimentPreference item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentPreferenceRequiredCodeRule(string Message) : ISentimentPreferenceRule
{
    public List<string> Check(SentimentPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment preference.</summary>
public sealed record SentimentPreferenceValidator(List<ISentimentPreferenceRule> Rules)
{
    public List<string> Validate(SentimentPreference item)
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

    public void RequireValid(SentimentPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentPreference> items)
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

/// <summary>Functions of sentiment preference rules.</summary>
public static class SentimentPreferenceRules
{
    public static SentimentPreferenceValidator DefaultSentimentPreferenceValidator()
    {
        return new SentimentPreferenceValidator(
            new List<ISentimentPreferenceRule>
            {
                new SentimentPreferenceCodePrefixRule("REF"),
                new SentimentPreferenceRequiredTitleRule("sentiment preference needs an entry"),
                new SentimentPreferenceExternalRefLengthRule(16),
                new SentimentPreferenceRequiredCodeRule("sentiment preference needs a value"),
            });
    }
}
