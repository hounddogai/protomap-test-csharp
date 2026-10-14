namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

/// <summary>A rule a sentiment agreement must pass. Check returns its problems, or none.</summary>
public interface ISentimentAgreementRule
{
    List<string> Check(SentimentAgreement item);
}

public sealed record SentimentAgreementCodePrefixRule(string Prefix) : ISentimentAgreementRule
{
    public List<string> Check(SentimentAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentAgreementJobCodeLengthRule(int MaxLength) : ISentimentAgreementRule
{
    public List<string> Check(SentimentAgreement item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentAgreementUpdatedAtRangeRule(int Min, int Max) : ISentimentAgreementRule
{
    public List<string> Check(SentimentAgreement item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a sentiment agreement.</summary>
public sealed record SentimentAgreementValidator(List<ISentimentAgreementRule> Rules)
{
    public List<string> Validate(SentimentAgreement item)
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

    public void RequireValid(SentimentAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentAgreement> items)
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

/// <summary>Functions of sentiment agreement rules.</summary>
public static class SentimentAgreementRules
{
    public static SentimentAgreementValidator DefaultSentimentAgreementValidator()
    {
        return new SentimentAgreementValidator(
            new List<ISentimentAgreementRule>
            {
                new SentimentAgreementCodePrefixRule("OPS"),
                new SentimentAgreementJobCodeLengthRule(80),
                new SentimentAgreementUpdatedAtRangeRule(64, 3600),
            });
    }
}
