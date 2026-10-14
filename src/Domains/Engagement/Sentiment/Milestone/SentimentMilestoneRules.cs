namespace Acme.Workforce.Engagement.Sentiment.Milestone;

/// <summary>A rule a sentiment milestone must pass. Check returns its problems, or none.</summary>
public interface ISentimentMilestoneRule
{
    List<string> Check(SentimentMilestone item);
}

public sealed record SentimentMilestoneCodePrefixRule(string Prefix) : ISentimentMilestoneRule
{
    public List<string> Check(SentimentMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentMilestoneRequiredNotesRule(string Message) : ISentimentMilestoneRule
{
    public List<string> Check(SentimentMilestone item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentMilestoneUpdatedAtRangeRule(int Min, int Max) : ISentimentMilestoneRule
{
    public List<string> Check(SentimentMilestone item)
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

/// <summary>Runs every rule of a sentiment milestone.</summary>
public sealed record SentimentMilestoneValidator(List<ISentimentMilestoneRule> Rules)
{
    public List<string> Validate(SentimentMilestone item)
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

    public void RequireValid(SentimentMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentMilestone> items)
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

/// <summary>Functions of sentiment milestone rules.</summary>
public static class SentimentMilestoneRules
{
    public static SentimentMilestoneValidator DefaultSentimentMilestoneValidator()
    {
        return new SentimentMilestoneValidator(
            new List<ISentimentMilestoneRule>
            {
                new SentimentMilestoneCodePrefixRule("X"),
                new SentimentMilestoneRequiredNotesRule("sentiment milestone needs this field"),
                new SentimentMilestoneUpdatedAtRangeRule(120, 1000),
            });
    }
}
