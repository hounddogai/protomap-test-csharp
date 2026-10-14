namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

/// <summary>A rule a tiered sentiment manifest must pass. Check returns its problems, or none.</summary>
public interface ITieredSentimentManifestRule
{
    List<string> Check(TieredSentimentManifest item);
}

public sealed record TieredSentimentManifestRequiredOwnerIdRule(string Message) : ITieredSentimentManifestRule
{
    public List<string> Check(TieredSentimentManifest item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentManifestExpiresAtRangeRule(int Min, int Max) : ITieredSentimentManifestRule
{
    public List<string> Check(TieredSentimentManifest item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentManifestOwnerIdLengthRule(int MaxLength) : ITieredSentimentManifestRule
{
    public List<string> Check(TieredSentimentManifest item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered sentiment manifest.</summary>
public sealed record TieredSentimentManifestValidator(List<ITieredSentimentManifestRule> Rules)
{
    public List<string> Validate(TieredSentimentManifest item)
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

    public void RequireValid(TieredSentimentManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredSentimentManifest> items)
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

/// <summary>Functions of tiered sentiment manifest rules.</summary>
public static class TieredSentimentManifestRules
{
    public static TieredSentimentManifestValidator DefaultTieredSentimentManifestValidator()
    {
        return new TieredSentimentManifestValidator(
            new List<ITieredSentimentManifestRule>
            {
                new TieredSentimentManifestRequiredOwnerIdRule("tiered sentiment manifest needs an entry"),
                new TieredSentimentManifestExpiresAtRangeRule(64, 3600),
                new TieredSentimentManifestOwnerIdLengthRule(80),
            });
    }
}
