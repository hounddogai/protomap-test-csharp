namespace Acme.Workforce.Engagement.Sentiment.Manifest;

/// <summary>A rule a sentiment manifest must pass. Check returns its problems, or none.</summary>
public interface ISentimentManifestRule
{
    List<string> Check(SentimentManifest item);
}

public sealed record SentimentManifestCodePrefixRule(string Prefix) : ISentimentManifestRule
{
    public List<string> Check(SentimentManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentManifestCodeLengthRule(int MaxLength) : ISentimentManifestRule
{
    public List<string> Check(SentimentManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentManifestRequiredExternalRefRule(string Message) : ISentimentManifestRule
{
    public List<string> Check(SentimentManifest item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentManifestRequiredCodeRule(string Message) : ISentimentManifestRule
{
    public List<string> Check(SentimentManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment manifest.</summary>
public sealed record SentimentManifestValidator(List<ISentimentManifestRule> Rules)
{
    public List<string> Validate(SentimentManifest item)
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

    public void RequireValid(SentimentManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentManifest> items)
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

/// <summary>Functions of sentiment manifest rules.</summary>
public static class SentimentManifestRules
{
    public static SentimentManifestValidator DefaultSentimentManifestValidator()
    {
        return new SentimentManifestValidator(
            new List<ISentimentManifestRule>
            {
                new SentimentManifestCodePrefixRule("OPS"),
                new SentimentManifestCodeLengthRule(86400),
                new SentimentManifestRequiredExternalRefRule("sentiment manifest needs an entry"),
                new SentimentManifestRequiredCodeRule("sentiment manifest needs this field"),
            });
    }
}
