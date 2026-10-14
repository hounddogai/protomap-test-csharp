namespace Acme.Workforce.Engagement.Sentiment.Target;

/// <summary>A rule a sentiment estimate must pass. Check returns its problems, or none.</summary>
public interface ISentimentEstimateRule
{
    List<string> Check(SentimentEstimate item);
}

public sealed record SentimentEstimateChannelLengthRule(int MaxLength) : ISentimentEstimateRule
{
    public List<string> Check(SentimentEstimate item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentEstimateRetriesRangeRule(int Min, int Max) : ISentimentEstimateRule
{
    public List<string> Check(SentimentEstimate item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentEstimateRequiredCodeRule(string Message) : ISentimentEstimateRule
{
    public List<string> Check(SentimentEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentEstimateRequiredDepartmentRule(string Message) : ISentimentEstimateRule
{
    public List<string> Check(SentimentEstimate item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentEstimateLabelLengthRule(int MaxLength) : ISentimentEstimateRule
{
    public List<string> Check(SentimentEstimate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentEstimateRateRangeRule(double Min, double Max) : ISentimentEstimateRule
{
    public List<string> Check(SentimentEstimate item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment estimate.</summary>
public sealed record SentimentEstimateValidator(List<ISentimentEstimateRule> Rules)
{
    public List<string> Validate(SentimentEstimate item)
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

    public void RequireValid(SentimentEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentEstimate> items)
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

/// <summary>Functions of sentiment estimate rules.</summary>
public static class SentimentEstimateRules
{
    public static SentimentEstimateValidator DefaultSentimentEstimateValidator()
    {
        return new SentimentEstimateValidator(
            new List<ISentimentEstimateRule>
            {
                new SentimentEstimateChannelLengthRule(86400),
                new SentimentEstimateRetriesRangeRule(120, 1000),
                new SentimentEstimateRequiredCodeRule("sentiment estimate needs this field"),
                new SentimentEstimateRequiredDepartmentRule("sentiment estimate needs this field"),
                new SentimentEstimateLabelLengthRule(32),
                new SentimentEstimateRateRangeRule(0.8, 546.2),
            });
    }
}
