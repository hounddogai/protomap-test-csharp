namespace Acme.Workforce.Engagement.Sentiment.Threshold;

/// <summary>A rule a fallback sentiment score must pass. Check returns its problems, or none.</summary>
public interface IFallbackSentimentScoreRule
{
    List<string> Check(FallbackSentimentScore item);
}

public sealed record FallbackSentimentScoreVersionRangeRule(int Min, int Max) : IFallbackSentimentScoreRule
{
    public List<string> Check(FallbackSentimentScore item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackSentimentScoreRequiredCountryRule(string Message) : IFallbackSentimentScoreRule
{
    public List<string> Check(FallbackSentimentScore item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentScoreLabelLengthRule(int MaxLength) : IFallbackSentimentScoreRule
{
    public List<string> Check(FallbackSentimentScore item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentScoreRequiredCodeRule(string Message) : IFallbackSentimentScoreRule
{
    public List<string> Check(FallbackSentimentScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentScoreLeaveDaysRangeRule(int Min, int Max) : IFallbackSentimentScoreRule
{
    public List<string> Check(FallbackSentimentScore item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackSentimentScoreCountryLengthRule(int MaxLength) : IFallbackSentimentScoreRule
{
    public List<string> Check(FallbackSentimentScore item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback sentiment score.</summary>
public sealed record FallbackSentimentScoreValidator(List<IFallbackSentimentScoreRule> Rules)
{
    public List<string> Validate(FallbackSentimentScore item)
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

    public void RequireValid(FallbackSentimentScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackSentimentScore> items)
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

/// <summary>Functions of fallback sentiment score rules.</summary>
public static class FallbackSentimentScoreRules
{
    public static FallbackSentimentScoreValidator DefaultFallbackSentimentScoreValidator()
    {
        return new FallbackSentimentScoreValidator(
            new List<IFallbackSentimentScoreRule>
            {
                new FallbackSentimentScoreVersionRangeRule(120, 255),
                new FallbackSentimentScoreRequiredCountryRule("fallback sentiment score needs a value"),
                new FallbackSentimentScoreLabelLengthRule(255),
                new FallbackSentimentScoreRequiredCodeRule("fallback sentiment score needs an entry"),
                new FallbackSentimentScoreLeaveDaysRangeRule(120, 120),
                new FallbackSentimentScoreCountryLengthRule(1000),
            });
    }
}
