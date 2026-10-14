namespace Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>A rule a sentiment rollup must pass. Check returns its problems, or none.</summary>
public interface ISentimentRollupRule
{
    List<string> Check(SentimentRollup item);
}

public sealed record SentimentRollupRequiredCodeRule(string Message) : ISentimentRollupRule
{
    public List<string> Check(SentimentRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentRollupRequiredBadgeRule(string Message) : ISentimentRollupRule
{
    public List<string> Check(SentimentRollup item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentRollupChannelLengthRule(int MaxLength) : ISentimentRollupRule
{
    public List<string> Check(SentimentRollup item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentRollupTitleLengthRule(int MaxLength) : ISentimentRollupRule
{
    public List<string> Check(SentimentRollup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment rollup.</summary>
public sealed record SentimentRollupValidator(List<ISentimentRollupRule> Rules)
{
    public List<string> Validate(SentimentRollup item)
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

    public void RequireValid(SentimentRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentRollup> items)
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

/// <summary>Functions of sentiment rollup rules.</summary>
public static class SentimentRollupRules
{
    public static SentimentRollupValidator DefaultSentimentRollupValidator()
    {
        return new SentimentRollupValidator(
            new List<ISentimentRollupRule>
            {
                new SentimentRollupRequiredCodeRule("sentiment rollup needs this field"),
                new SentimentRollupRequiredBadgeRule("sentiment rollup needs a value"),
                new SentimentRollupChannelLengthRule(255),
                new SentimentRollupTitleLengthRule(32),
            });
    }
}
