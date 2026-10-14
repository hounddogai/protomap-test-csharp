namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment target must pass. Check returns its problems, or none.</summary>
public interface ISentimentTargetRule
{
    List<string> Check(SentimentTarget item);
}

public sealed record SentimentTargetRequiredDescriptionRule(string Message) : ISentimentTargetRule
{
    public List<string> Check(SentimentTarget item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentTargetAllowedSummaryRule(List<string> Allowed) : ISentimentTargetRule
{
    public List<string> Check(SentimentTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record SentimentTargetRequiredChecksumRule(string Message) : ISentimentTargetRule
{
    public List<string> Check(SentimentTarget item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentTargetRequiredTitleRule(string Message) : ISentimentTargetRule
{
    public List<string> Check(SentimentTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment target.</summary>
public sealed record SentimentTargetValidator(List<ISentimentTargetRule> Rules)
{
    public List<string> Validate(SentimentTarget item)
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

    public void RequireValid(SentimentTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentTarget> items)
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

/// <summary>Functions of sentiment target rules.</summary>
public static class SentimentTargetRules
{
    public static SentimentTargetValidator DefaultSentimentTargetValidator()
    {
        return new SentimentTargetValidator(
            new List<ISentimentTargetRule>
            {
                new SentimentTargetRequiredDescriptionRule("sentiment target needs an entry"),
                new SentimentTargetAllowedSummaryRule(new List<string> { "seasonal", "capped", "fallback" }),
                new SentimentTargetRequiredChecksumRule("sentiment target needs this field"),
                new SentimentTargetRequiredTitleRule("sentiment target needs a value"),
            });
    }
}
