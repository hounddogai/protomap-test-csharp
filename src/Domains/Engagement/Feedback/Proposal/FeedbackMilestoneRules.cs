namespace Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>A rule a feedback milestone must pass. Check returns its problems, or none.</summary>
public interface IFeedbackMilestoneRule
{
    List<string> Check(FeedbackMilestone item);
}

public sealed record FeedbackMilestoneRequiredTitleRule(string Message) : IFeedbackMilestoneRule
{
    public List<string> Check(FeedbackMilestone item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackMilestonePriorityRangeRule(int Min, int Max) : IFeedbackMilestoneRule
{
    public List<string> Check(FeedbackMilestone item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackMilestoneSalaryBandLengthRule(int MaxLength) : IFeedbackMilestoneRule
{
    public List<string> Check(FeedbackMilestone item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackMilestoneRequiredCodeRule(string Message) : IFeedbackMilestoneRule
{
    public List<string> Check(FeedbackMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackMilestoneRequiredLocationRule(string Message) : IFeedbackMilestoneRule
{
    public List<string> Check(FeedbackMilestone item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback milestone.</summary>
public sealed record FeedbackMilestoneValidator(List<IFeedbackMilestoneRule> Rules)
{
    public List<string> Validate(FeedbackMilestone item)
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

    public void RequireValid(FeedbackMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackMilestone> items)
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

/// <summary>Functions of feedback milestone rules.</summary>
public static class FeedbackMilestoneRules
{
    public static FeedbackMilestoneValidator DefaultFeedbackMilestoneValidator()
    {
        return new FeedbackMilestoneValidator(
            new List<IFeedbackMilestoneRule>
            {
                new FeedbackMilestoneRequiredTitleRule("feedback milestone needs an entry"),
                new FeedbackMilestonePriorityRangeRule(80, 255),
                new FeedbackMilestoneSalaryBandLengthRule(255),
                new FeedbackMilestoneRequiredCodeRule("feedback milestone needs this field"),
                new FeedbackMilestoneRequiredLocationRule("feedback milestone needs a value"),
            });
    }
}
