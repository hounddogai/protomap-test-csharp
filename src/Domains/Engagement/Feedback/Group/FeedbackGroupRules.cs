namespace Acme.Workforce.Engagement.Feedback.Group;

/// <summary>A rule a feedback group must pass. Check returns its problems, or none.</summary>
public interface IFeedbackGroupRule
{
    List<string> Check(FeedbackGroup item);
}

public sealed record FeedbackGroupVersionRangeRule(int Min, int Max) : IFeedbackGroupRule
{
    public List<string> Check(FeedbackGroup item)
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

public sealed record FeedbackGroupRequiredNameRule(string Message) : IFeedbackGroupRule
{
    public List<string> Check(FeedbackGroup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackGroupChecksumLengthRule(int MaxLength) : IFeedbackGroupRule
{
    public List<string> Check(FeedbackGroup item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackGroupRequiredCodeRule(string Message) : IFeedbackGroupRule
{
    public List<string> Check(FeedbackGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback group.</summary>
public sealed record FeedbackGroupValidator(List<IFeedbackGroupRule> Rules)
{
    public List<string> Validate(FeedbackGroup item)
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

    public void RequireValid(FeedbackGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackGroup> items)
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

/// <summary>Functions of feedback group rules.</summary>
public static class FeedbackGroupRules
{
    public static FeedbackGroupValidator DefaultFeedbackGroupValidator()
    {
        return new FeedbackGroupValidator(
            new List<IFeedbackGroupRule>
            {
                new FeedbackGroupVersionRangeRule(64, 64),
                new FeedbackGroupRequiredNameRule("feedback group needs an entry"),
                new FeedbackGroupChecksumLengthRule(16),
                new FeedbackGroupRequiredCodeRule("feedback group needs this field"),
            });
    }
}
