namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>A rule a sentiment digest must pass. Check returns its problems, or none.</summary>
public interface ISentimentDigestRule
{
    List<string> Check(SentimentDigest item);
}

public sealed record SentimentDigestCodeLengthRule(int MaxLength) : ISentimentDigestRule
{
    public List<string> Check(SentimentDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentDigestDescriptionLengthRule(int MaxLength) : ISentimentDigestRule
{
    public List<string> Check(SentimentDigest item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentDigestSequenceRangeRule(int Min, int Max) : ISentimentDigestRule
{
    public List<string> Check(SentimentDigest item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentDigestCodePrefixRule(string Prefix) : ISentimentDigestRule
{
    public List<string> Check(SentimentDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentDigestSalaryBandLengthRule(int MaxLength) : ISentimentDigestRule
{
    public List<string> Check(SentimentDigest item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment digest.</summary>
public sealed record SentimentDigestValidator(List<ISentimentDigestRule> Rules)
{
    public List<string> Validate(SentimentDigest item)
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

    public void RequireValid(SentimentDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentDigest> items)
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

/// <summary>Functions of sentiment digest rules.</summary>
public static class SentimentDigestRules
{
    public static SentimentDigestValidator DefaultSentimentDigestValidator()
    {
        return new SentimentDigestValidator(
            new List<ISentimentDigestRule>
            {
                new SentimentDigestCodeLengthRule(80),
                new SentimentDigestDescriptionLengthRule(16),
                new SentimentDigestSequenceRangeRule(255, 1000),
                new SentimentDigestCodePrefixRule("OPS"),
                new SentimentDigestSalaryBandLengthRule(80),
            });
    }
}
