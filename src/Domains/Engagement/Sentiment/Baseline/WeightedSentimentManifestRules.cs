namespace Acme.Workforce.Engagement.Sentiment.Baseline;

/// <summary>A rule a weighted sentiment manifest must pass. Check returns its problems, or none.</summary>
public interface IWeightedSentimentManifestRule
{
    List<string> Check(WeightedSentimentManifest item);
}

public sealed record WeightedSentimentManifestVisibleGuardRule(int Limit) : IWeightedSentimentManifestRule
{
    public List<string> Check(WeightedSentimentManifest item)
    {
        if (item.Visible && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record WeightedSentimentManifestCodePrefixRule(string Prefix) : IWeightedSentimentManifestRule
{
    public List<string> Check(WeightedSentimentManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedSentimentManifestMarginRangeRule(double Min, double Max) : IWeightedSentimentManifestRule
{
    public List<string> Check(WeightedSentimentManifest item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted sentiment manifest.</summary>
public sealed record WeightedSentimentManifestValidator(List<IWeightedSentimentManifestRule> Rules)
{
    public List<string> Validate(WeightedSentimentManifest item)
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

    public void RequireValid(WeightedSentimentManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedSentimentManifest> items)
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

/// <summary>Functions of weighted sentiment manifest rules.</summary>
public static class WeightedSentimentManifestRules
{
    public static WeightedSentimentManifestValidator DefaultWeightedSentimentManifestValidator()
    {
        return new WeightedSentimentManifestValidator(
            new List<IWeightedSentimentManifestRule>
            {
                new WeightedSentimentManifestVisibleGuardRule(64),
                new WeightedSentimentManifestCodePrefixRule("Q"),
                new WeightedSentimentManifestMarginRangeRule(5.3, 976.1),
            });
    }
}
