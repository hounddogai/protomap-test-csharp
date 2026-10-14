namespace Acme.Workforce.Engagement.Feedback.Checkpoint;

/// <summary>A rule a seasonal feedback milestone must pass. Check returns its problems, or none.</summary>
public interface ISeasonalFeedbackMilestoneRule
{
    List<string> Check(SeasonalFeedbackMilestone item);
}

public sealed record SeasonalFeedbackMilestoneManagerIdLengthRule(int MaxLength) : ISeasonalFeedbackMilestoneRule
{
    public List<string> Check(SeasonalFeedbackMilestone item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackMilestoneNameLengthRule(int MaxLength) : ISeasonalFeedbackMilestoneRule
{
    public List<string> Check(SeasonalFeedbackMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackMilestoneUpdatedAtRangeRule(int Min, int Max) : ISeasonalFeedbackMilestoneRule
{
    public List<string> Check(SeasonalFeedbackMilestone item)
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

/// <summary>Runs every rule of a seasonal feedback milestone.</summary>
public sealed record SeasonalFeedbackMilestoneValidator(List<ISeasonalFeedbackMilestoneRule> Rules)
{
    public List<string> Validate(SeasonalFeedbackMilestone item)
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

    public void RequireValid(SeasonalFeedbackMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalFeedbackMilestone> items)
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

/// <summary>Functions of seasonal feedback milestone rules.</summary>
public static class SeasonalFeedbackMilestoneRules
{
    public static SeasonalFeedbackMilestoneValidator DefaultSeasonalFeedbackMilestoneValidator()
    {
        return new SeasonalFeedbackMilestoneValidator(
            new List<ISeasonalFeedbackMilestoneRule>
            {
                new SeasonalFeedbackMilestoneManagerIdLengthRule(1000),
                new SeasonalFeedbackMilestoneNameLengthRule(1000),
                new SeasonalFeedbackMilestoneUpdatedAtRangeRule(64, 3600),
            });
    }
}
