namespace Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>A rule a sentiment summary must pass. Check returns its problems, or none.</summary>
public interface ISentimentSummaryRule
{
    List<string> Check(SentimentSummary item);
}

public sealed record SentimentSummaryRequiredTitleRule(string Message) : ISentimentSummaryRule
{
    public List<string> Check(SentimentSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentSummaryCodeLengthRule(int MaxLength) : ISentimentSummaryRule
{
    public List<string> Check(SentimentSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentSummaryCreatedAtRangeRule(int Min, int Max) : ISentimentSummaryRule
{
    public List<string> Check(SentimentSummary item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SentimentSummaryTitleLengthRule(int MaxLength) : ISentimentSummaryRule
{
    public List<string> Check(SentimentSummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentSummaryRequiredDescriptionRule(string Message) : ISentimentSummaryRule
{
    public List<string> Check(SentimentSummary item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment summary.</summary>
public sealed record SentimentSummaryValidator(List<ISentimentSummaryRule> Rules)
{
    public List<string> Validate(SentimentSummary item)
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

    public void RequireValid(SentimentSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentSummary> items)
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

/// <summary>Functions of sentiment summary rules.</summary>
public static class SentimentSummaryRules
{
    public static SentimentSummaryValidator DefaultSentimentSummaryValidator()
    {
        return new SentimentSummaryValidator(
            new List<ISentimentSummaryRule>
            {
                new SentimentSummaryRequiredTitleRule("sentiment summary needs an entry"),
                new SentimentSummaryCodeLengthRule(80),
                new SentimentSummaryCreatedAtRangeRule(80, 120),
                new SentimentSummaryTitleLengthRule(86400),
                new SentimentSummaryRequiredDescriptionRule("sentiment summary needs this field"),
            });
    }
}
