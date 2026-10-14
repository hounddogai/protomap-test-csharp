namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>A rule a regional sentiment milestone must pass. Check returns its problems, or none.</summary>
public interface IRegionalSentimentMilestoneRule
{
    List<string> Check(RegionalSentimentMilestone item);
}

public sealed record RegionalSentimentMilestoneCodePrefixRule(string Prefix) : IRegionalSentimentMilestoneRule
{
    public List<string> Check(RegionalSentimentMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSentimentMilestoneLabelLengthRule(int MaxLength) : IRegionalSentimentMilestoneRule
{
    public List<string> Check(RegionalSentimentMilestone item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSentimentMilestoneRequiredContactEmailRule(string Message) : IRegionalSentimentMilestoneRule
{
    public List<string> Check(RegionalSentimentMilestone item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSentimentMilestonePriorityRangeRule(int Min, int Max) : IRegionalSentimentMilestoneRule
{
    public List<string> Check(RegionalSentimentMilestone item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional sentiment milestone.</summary>
public sealed record RegionalSentimentMilestoneValidator(List<IRegionalSentimentMilestoneRule> Rules)
{
    public List<string> Validate(RegionalSentimentMilestone item)
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

    public void RequireValid(RegionalSentimentMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSentimentMilestone> items)
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

/// <summary>Functions of regional sentiment milestone rules.</summary>
public static class RegionalSentimentMilestoneRules
{
    public static RegionalSentimentMilestoneValidator DefaultRegionalSentimentMilestoneValidator()
    {
        return new RegionalSentimentMilestoneValidator(
            new List<IRegionalSentimentMilestoneRule>
            {
                new RegionalSentimentMilestoneCodePrefixRule("X"),
                new RegionalSentimentMilestoneLabelLengthRule(255),
                new RegionalSentimentMilestoneRequiredContactEmailRule("regional sentiment milestone needs a value"),
                new RegionalSentimentMilestonePriorityRangeRule(16, 255),
            });
    }
}
