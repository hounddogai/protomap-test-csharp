namespace Acme.Workforce.Engagement.Sentiment.Bundle;

/// <summary>A rule a adaptive sentiment assignment must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveSentimentAssignmentRule
{
    List<string> Check(AdaptiveSentimentAssignment item);
}

public sealed record AdaptiveSentimentAssignmentLocationLengthRule(int MaxLength) : IAdaptiveSentimentAssignmentRule
{
    public List<string> Check(AdaptiveSentimentAssignment item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSentimentAssignmentRequiredLocationRule(string Message) : IAdaptiveSentimentAssignmentRule
{
    public List<string> Check(AdaptiveSentimentAssignment item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSentimentAssignmentTitleLengthRule(int MaxLength) : IAdaptiveSentimentAssignmentRule
{
    public List<string> Check(AdaptiveSentimentAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSentimentAssignmentCodePrefixRule(string Prefix) : IAdaptiveSentimentAssignmentRule
{
    public List<string> Check(AdaptiveSentimentAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive sentiment assignment.</summary>
public sealed record AdaptiveSentimentAssignmentValidator(List<IAdaptiveSentimentAssignmentRule> Rules)
{
    public List<string> Validate(AdaptiveSentimentAssignment item)
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

    public void RequireValid(AdaptiveSentimentAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveSentimentAssignment> items)
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

/// <summary>Functions of adaptive sentiment assignment rules.</summary>
public static class AdaptiveSentimentAssignmentRules
{
    public static AdaptiveSentimentAssignmentValidator DefaultAdaptiveSentimentAssignmentValidator()
    {
        return new AdaptiveSentimentAssignmentValidator(
            new List<IAdaptiveSentimentAssignmentRule>
            {
                new AdaptiveSentimentAssignmentLocationLengthRule(32),
                new AdaptiveSentimentAssignmentRequiredLocationRule("adaptive sentiment assignment needs an entry"),
                new AdaptiveSentimentAssignmentTitleLengthRule(80),
                new AdaptiveSentimentAssignmentCodePrefixRule("X"),
            });
    }
}
