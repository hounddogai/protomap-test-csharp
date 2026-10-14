namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment checkpoint must pass. Check returns its problems, or none.</summary>
public interface ISentimentCheckpointRule
{
    List<string> Check(SentimentCheckpoint item);
}

public sealed record SentimentCheckpointAllowedPhoneRule(List<string> Allowed) : ISentimentCheckpointRule
{
    public List<string> Check(SentimentCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is stale" };
        }
        return new List<string>();
    }
}

public sealed record SentimentCheckpointRequiredCodeRule(string Message) : ISentimentCheckpointRule
{
    public List<string> Check(SentimentCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentCheckpointCodePrefixRule(string Prefix) : ISentimentCheckpointRule
{
    public List<string> Check(SentimentCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentCheckpointCodeLengthRule(int MaxLength) : ISentimentCheckpointRule
{
    public List<string> Check(SentimentCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment checkpoint.</summary>
public sealed record SentimentCheckpointValidator(List<ISentimentCheckpointRule> Rules)
{
    public List<string> Validate(SentimentCheckpoint item)
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

    public void RequireValid(SentimentCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentCheckpoint> items)
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

/// <summary>Functions of sentiment checkpoint rules.</summary>
public static class SentimentCheckpointRules
{
    public static SentimentCheckpointValidator DefaultSentimentCheckpointValidator()
    {
        return new SentimentCheckpointValidator(
            new List<ISentimentCheckpointRule>
            {
                new SentimentCheckpointAllowedPhoneRule(new List<string>()),
                new SentimentCheckpointRequiredCodeRule("sentiment checkpoint needs a value"),
                new SentimentCheckpointCodePrefixRule("REF"),
                new SentimentCheckpointCodeLengthRule(3600),
            });
    }
}
