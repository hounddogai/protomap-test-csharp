namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>A rule a sentiment variance must pass. Check returns its problems, or none.</summary>
public interface ISentimentVarianceRule
{
    List<string> Check(SentimentVariance item);
}

public sealed record SentimentVarianceRequiredTitleRule(string Message) : ISentimentVarianceRule
{
    public List<string> Check(SentimentVariance item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentVarianceTitleLengthRule(int MaxLength) : ISentimentVarianceRule
{
    public List<string> Check(SentimentVariance item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentVarianceCodeLengthRule(int MaxLength) : ISentimentVarianceRule
{
    public List<string> Check(SentimentVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment variance.</summary>
public sealed record SentimentVarianceValidator(List<ISentimentVarianceRule> Rules)
{
    public List<string> Validate(SentimentVariance item)
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

    public void RequireValid(SentimentVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentVariance> items)
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

/// <summary>Functions of sentiment variance rules.</summary>
public static class SentimentVarianceRules
{
    public static SentimentVarianceValidator DefaultSentimentVarianceValidator()
    {
        return new SentimentVarianceValidator(
            new List<ISentimentVarianceRule>
            {
                new SentimentVarianceRequiredTitleRule("sentiment variance needs a value"),
                new SentimentVarianceTitleLengthRule(1000),
                new SentimentVarianceCodeLengthRule(255),
            });
    }
}
