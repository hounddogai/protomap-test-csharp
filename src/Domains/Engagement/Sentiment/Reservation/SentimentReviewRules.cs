namespace Acme.Workforce.Engagement.Sentiment.Reservation;

/// <summary>A rule a sentiment review must pass. Check returns its problems, or none.</summary>
public interface ISentimentReviewRule
{
    List<string> Check(SentimentReview item);
}

public sealed record SentimentReviewRequiredCurrencyRule(string Message) : ISentimentReviewRule
{
    public List<string> Check(SentimentReview item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentReviewLimitRangeRule(int Min, int Max) : ISentimentReviewRule
{
    public List<string> Check(SentimentReview item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SentimentReviewRequiredLocaleRule(string Message) : ISentimentReviewRule
{
    public List<string> Check(SentimentReview item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentReviewCodePrefixRule(string Prefix) : ISentimentReviewRule
{
    public List<string> Check(SentimentReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentReviewLocaleLengthRule(int MaxLength) : ISentimentReviewRule
{
    public List<string> Check(SentimentReview item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentReviewNameLengthRule(int MaxLength) : ISentimentReviewRule
{
    public List<string> Check(SentimentReview item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment review.</summary>
public sealed record SentimentReviewValidator(List<ISentimentReviewRule> Rules)
{
    public List<string> Validate(SentimentReview item)
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

    public void RequireValid(SentimentReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentReview> items)
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

/// <summary>Functions of sentiment review rules.</summary>
public static class SentimentReviewRules
{
    public static SentimentReviewValidator DefaultSentimentReviewValidator()
    {
        return new SentimentReviewValidator(
            new List<ISentimentReviewRule>
            {
                new SentimentReviewRequiredCurrencyRule("sentiment review needs this field"),
                new SentimentReviewLimitRangeRule(64, 86400),
                new SentimentReviewRequiredLocaleRule("sentiment review needs a value"),
                new SentimentReviewCodePrefixRule("OPS"),
                new SentimentReviewLocaleLengthRule(16),
                new SentimentReviewNameLengthRule(86400),
            });
    }
}
