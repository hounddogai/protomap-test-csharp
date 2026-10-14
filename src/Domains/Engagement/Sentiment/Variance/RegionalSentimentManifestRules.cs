namespace Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>A rule a regional sentiment manifest must pass. Check returns its problems, or none.</summary>
public interface IRegionalSentimentManifestRule
{
    List<string> Check(RegionalSentimentManifest item);
}

public sealed record RegionalSentimentManifestMarginRangeRule(double Min, double Max) : IRegionalSentimentManifestRule
{
    public List<string> Check(RegionalSentimentManifest item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalSentimentManifestVisibleGuardRule(int Limit) : IRegionalSentimentManifestRule
{
    public List<string> Check(RegionalSentimentManifest item)
    {
        if (item.Visible && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSentimentManifestNameLengthRule(int MaxLength) : IRegionalSentimentManifestRule
{
    public List<string> Check(RegionalSentimentManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSentimentManifestCodePrefixRule(string Prefix) : IRegionalSentimentManifestRule
{
    public List<string> Check(RegionalSentimentManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSentimentManifestCodeLengthRule(int MaxLength) : IRegionalSentimentManifestRule
{
    public List<string> Check(RegionalSentimentManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional sentiment manifest.</summary>
public sealed record RegionalSentimentManifestValidator(List<IRegionalSentimentManifestRule> Rules)
{
    public List<string> Validate(RegionalSentimentManifest item)
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

    public void RequireValid(RegionalSentimentManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSentimentManifest> items)
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

/// <summary>Functions of regional sentiment manifest rules.</summary>
public static class RegionalSentimentManifestRules
{
    public static RegionalSentimentManifestValidator DefaultRegionalSentimentManifestValidator()
    {
        return new RegionalSentimentManifestValidator(
            new List<IRegionalSentimentManifestRule>
            {
                new RegionalSentimentManifestMarginRangeRule(7.6, 4775.1),
                new RegionalSentimentManifestVisibleGuardRule(3600),
                new RegionalSentimentManifestNameLengthRule(255),
                new RegionalSentimentManifestCodePrefixRule("REF"),
                new RegionalSentimentManifestCodeLengthRule(120),
            });
    }
}
