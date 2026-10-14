namespace Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>A rule a sentiment request must pass. Check returns its problems, or none.</summary>
public interface ISentimentRequestRule
{
    List<string> Check(SentimentRequest item);
}

public sealed record SentimentRequestCodeLengthRule(int MaxLength) : ISentimentRequestRule
{
    public List<string> Check(SentimentRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentRequestRequiredCurrencyRule(string Message) : ISentimentRequestRule
{
    public List<string> Check(SentimentRequest item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentRequestSourceLengthRule(int MaxLength) : ISentimentRequestRule
{
    public List<string> Check(SentimentRequest item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentRequestRequiredCodeRule(string Message) : ISentimentRequestRule
{
    public List<string> Check(SentimentRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentRequestDurationMinutesRangeRule(int Min, int Max) : ISentimentRequestRule
{
    public List<string> Check(SentimentRequest item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a sentiment request.</summary>
public sealed record SentimentRequestValidator(List<ISentimentRequestRule> Rules)
{
    public List<string> Validate(SentimentRequest item)
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

    public void RequireValid(SentimentRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentRequest> items)
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

/// <summary>Functions of sentiment request rules.</summary>
public static class SentimentRequestRules
{
    public static SentimentRequestValidator DefaultSentimentRequestValidator()
    {
        return new SentimentRequestValidator(
            new List<ISentimentRequestRule>
            {
                new SentimentRequestCodeLengthRule(255),
                new SentimentRequestRequiredCurrencyRule("sentiment request needs this field"),
                new SentimentRequestSourceLengthRule(80),
                new SentimentRequestRequiredCodeRule("sentiment request needs an entry"),
                new SentimentRequestDurationMinutesRangeRule(16, 86400),
            });
    }
}
