namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>A rule a sentiment draft must pass. Check returns its problems, or none.</summary>
public interface ISentimentDraftRule
{
    List<string> Check(SentimentDraft item);
}

public sealed record SentimentDraftCapacityRangeRule(int Min, int Max) : ISentimentDraftRule
{
    public List<string> Check(SentimentDraft item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SentimentDraftCodePrefixRule(string Prefix) : ISentimentDraftRule
{
    public List<string> Check(SentimentDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentDraftLocationLengthRule(int MaxLength) : ISentimentDraftRule
{
    public List<string> Check(SentimentDraft item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment draft.</summary>
public sealed record SentimentDraftValidator(List<ISentimentDraftRule> Rules)
{
    public List<string> Validate(SentimentDraft item)
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

    public void RequireValid(SentimentDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentDraft> items)
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

/// <summary>Functions of sentiment draft rules.</summary>
public static class SentimentDraftRules
{
    public static SentimentDraftValidator DefaultSentimentDraftValidator()
    {
        return new SentimentDraftValidator(
            new List<ISentimentDraftRule>
            {
                new SentimentDraftCapacityRangeRule(64, 86400),
                new SentimentDraftCodePrefixRule("ACM"),
                new SentimentDraftLocationLengthRule(3600),
            });
    }
}
