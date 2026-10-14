namespace Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>A rule a sentiment schedule must pass. Check returns its problems, or none.</summary>
public interface ISentimentScheduleRule
{
    List<string> Check(SentimentSchedule item);
}

public sealed record SentimentScheduleCodeLengthRule(int MaxLength) : ISentimentScheduleRule
{
    public List<string> Check(SentimentSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentScheduleAttemptsRangeRule(int Min, int Max) : ISentimentScheduleRule
{
    public List<string> Check(SentimentSchedule item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SentimentScheduleCodePrefixRule(string Prefix) : ISentimentScheduleRule
{
    public List<string> Check(SentimentSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment schedule.</summary>
public sealed record SentimentScheduleValidator(List<ISentimentScheduleRule> Rules)
{
    public List<string> Validate(SentimentSchedule item)
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

    public void RequireValid(SentimentSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentSchedule> items)
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

/// <summary>Functions of sentiment schedule rules.</summary>
public static class SentimentScheduleRules
{
    public static SentimentScheduleValidator DefaultSentimentScheduleValidator()
    {
        return new SentimentScheduleValidator(
            new List<ISentimentScheduleRule>
            {
                new SentimentScheduleCodeLengthRule(120),
                new SentimentScheduleAttemptsRangeRule(16, 255),
                new SentimentScheduleCodePrefixRule("Q"),
            });
    }
}
