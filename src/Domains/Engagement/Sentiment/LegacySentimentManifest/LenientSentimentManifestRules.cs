namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>A rule a lenient sentiment manifest must pass. Check returns its problems, or none.</summary>
public interface ILenientSentimentManifestRule
{
    List<string> Check(LenientSentimentManifest item);
}

public sealed record LenientSentimentManifestCodePrefixRule(string Prefix) : ILenientSentimentManifestRule
{
    public List<string> Check(LenientSentimentManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentManifestCodeLengthRule(int MaxLength) : ILenientSentimentManifestRule
{
    public List<string> Check(LenientSentimentManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentManifestRequiredCategoryRule(string Message) : ILenientSentimentManifestRule
{
    public List<string> Check(LenientSentimentManifest item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentManifestVersionRangeRule(int Min, int Max) : ILenientSentimentManifestRule
{
    public List<string> Check(LenientSentimentManifest item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentManifestDurationMinutesRangeRule(int Min, int Max) : ILenientSentimentManifestRule
{
    public List<string> Check(LenientSentimentManifest item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a lenient sentiment manifest.</summary>
public sealed record LenientSentimentManifestValidator(List<ILenientSentimentManifestRule> Rules)
{
    public List<string> Validate(LenientSentimentManifest item)
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

    public void RequireValid(LenientSentimentManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientSentimentManifest> items)
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

/// <summary>Functions of lenient sentiment manifest rules.</summary>
public static class LenientSentimentManifestRules
{
    public static LenientSentimentManifestValidator DefaultLenientSentimentManifestValidator()
    {
        return new LenientSentimentManifestValidator(
            new List<ILenientSentimentManifestRule>
            {
                new LenientSentimentManifestCodePrefixRule("Q"),
                new LenientSentimentManifestCodeLengthRule(64),
                new LenientSentimentManifestRequiredCategoryRule("lenient sentiment manifest needs a value"),
                new LenientSentimentManifestVersionRangeRule(64, 255),
                new LenientSentimentManifestDurationMinutesRangeRule(1000, 3600),
            });
    }
}
