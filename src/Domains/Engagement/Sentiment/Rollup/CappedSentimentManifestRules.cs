namespace Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>A rule a capped sentiment manifest must pass. Check returns its problems, or none.</summary>
public interface ICappedSentimentManifestRule
{
    List<string> Check(CappedSentimentManifest item);
}

public sealed record CappedSentimentManifestChannelLengthRule(int MaxLength) : ICappedSentimentManifestRule
{
    public List<string> Check(CappedSentimentManifest item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSentimentManifestTitleLengthRule(int MaxLength) : ICappedSentimentManifestRule
{
    public List<string> Check(CappedSentimentManifest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSentimentManifestCeilingRangeRule(double Min, double Max) : ICappedSentimentManifestRule
{
    public List<string> Check(CappedSentimentManifest item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedSentimentManifestRequiredCodeRule(string Message) : ICappedSentimentManifestRule
{
    public List<string> Check(CappedSentimentManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped sentiment manifest.</summary>
public sealed record CappedSentimentManifestValidator(List<ICappedSentimentManifestRule> Rules)
{
    public List<string> Validate(CappedSentimentManifest item)
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

    public void RequireValid(CappedSentimentManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedSentimentManifest> items)
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

/// <summary>Functions of capped sentiment manifest rules.</summary>
public static class CappedSentimentManifestRules
{
    public static CappedSentimentManifestValidator DefaultCappedSentimentManifestValidator()
    {
        return new CappedSentimentManifestValidator(
            new List<ICappedSentimentManifestRule>
            {
                new CappedSentimentManifestChannelLengthRule(120),
                new CappedSentimentManifestTitleLengthRule(120),
                new CappedSentimentManifestCeilingRangeRule(4.4, 2489.3),
                new CappedSentimentManifestRequiredCodeRule("capped sentiment manifest needs a value"),
            });
    }
}
