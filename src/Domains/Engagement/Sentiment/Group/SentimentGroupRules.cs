namespace Acme.Workforce.Engagement.Sentiment.Group;

/// <summary>A rule a sentiment group must pass. Check returns its problems, or none.</summary>
public interface ISentimentGroupRule
{
    List<string> Check(SentimentGroup item);
}

public sealed record SentimentGroupCodePrefixRule(string Prefix) : ISentimentGroupRule
{
    public List<string> Check(SentimentGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentGroupSkillLengthRule(int MaxLength) : ISentimentGroupRule
{
    public List<string> Check(SentimentGroup item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentGroupRequiredTitleRule(string Message) : ISentimentGroupRule
{
    public List<string> Check(SentimentGroup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentGroupSegmentLengthRule(int MaxLength) : ISentimentGroupRule
{
    public List<string> Check(SentimentGroup item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment group.</summary>
public sealed record SentimentGroupValidator(List<ISentimentGroupRule> Rules)
{
    public List<string> Validate(SentimentGroup item)
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

    public void RequireValid(SentimentGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentGroup> items)
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

/// <summary>Functions of sentiment group rules.</summary>
public static class SentimentGroupRules
{
    public static SentimentGroupValidator DefaultSentimentGroupValidator()
    {
        return new SentimentGroupValidator(
            new List<ISentimentGroupRule>
            {
                new SentimentGroupCodePrefixRule("X"),
                new SentimentGroupSkillLengthRule(255),
                new SentimentGroupRequiredTitleRule("sentiment group needs a value"),
                new SentimentGroupSegmentLengthRule(1000),
            });
    }
}
