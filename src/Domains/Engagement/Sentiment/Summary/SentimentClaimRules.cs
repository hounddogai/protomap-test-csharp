namespace Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>A rule a sentiment claim must pass. Check returns its problems, or none.</summary>
public interface ISentimentClaimRule
{
    List<string> Check(SentimentClaim item);
}

public sealed record SentimentClaimLabelLengthRule(int MaxLength) : ISentimentClaimRule
{
    public List<string> Check(SentimentClaim item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentClaimScoreRangeRule(double Min, double Max) : ISentimentClaimRule
{
    public List<string> Check(SentimentClaim item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentClaimActiveGuardRule(double Limit) : ISentimentClaimRule
{
    public List<string> Check(SentimentClaim item)
    {
        if (item.Active && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment claim.</summary>
public sealed record SentimentClaimValidator(List<ISentimentClaimRule> Rules)
{
    public List<string> Validate(SentimentClaim item)
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

    public void RequireValid(SentimentClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentClaim> items)
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

/// <summary>Functions of sentiment claim rules.</summary>
public static class SentimentClaimRules
{
    public static SentimentClaimValidator DefaultSentimentClaimValidator()
    {
        return new SentimentClaimValidator(
            new List<ISentimentClaimRule>
            {
                new SentimentClaimLabelLengthRule(32),
                new SentimentClaimScoreRangeRule(0.7, 2419.9),
                new SentimentClaimActiveGuardRule(2.9),
            });
    }
}
