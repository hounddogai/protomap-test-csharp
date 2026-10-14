namespace Acme.Workforce.Engagement.Sentiment.SentimentSummary;

/// <summary>A rule a sentiment bundle must pass. Check returns its problems, or none.</summary>
public interface ISentimentBundleRule
{
    List<string> Check(SentimentBundle item);
}

public sealed record SentimentBundleCodeLengthRule(int MaxLength) : ISentimentBundleRule
{
    public List<string> Check(SentimentBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentBundleRequiredCategoryRule(string Message) : ISentimentBundleRule
{
    public List<string> Check(SentimentBundle item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentBundleRequiredCodeRule(string Message) : ISentimentBundleRule
{
    public List<string> Check(SentimentBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentBundleReviewerLengthRule(int MaxLength) : ISentimentBundleRule
{
    public List<string> Check(SentimentBundle item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentBundleAmountRangeRule(double Min, double Max) : ISentimentBundleRule
{
    public List<string> Check(SentimentBundle item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SentimentBundleCodePrefixRule(string Prefix) : ISentimentBundleRule
{
    public List<string> Check(SentimentBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment bundle.</summary>
public sealed record SentimentBundleValidator(List<ISentimentBundleRule> Rules)
{
    public List<string> Validate(SentimentBundle item)
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

    public void RequireValid(SentimentBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentBundle> items)
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

/// <summary>Functions of sentiment bundle rules.</summary>
public static class SentimentBundleRules
{
    public static SentimentBundleValidator DefaultSentimentBundleValidator()
    {
        return new SentimentBundleValidator(
            new List<ISentimentBundleRule>
            {
                new SentimentBundleCodeLengthRule(255),
                new SentimentBundleRequiredCategoryRule("sentiment bundle needs a value"),
                new SentimentBundleRequiredCodeRule("sentiment bundle needs this field"),
                new SentimentBundleReviewerLengthRule(3600),
                new SentimentBundleAmountRangeRule(3.1, 4842.3),
                new SentimentBundleCodePrefixRule("ACM"),
            });
    }
}
