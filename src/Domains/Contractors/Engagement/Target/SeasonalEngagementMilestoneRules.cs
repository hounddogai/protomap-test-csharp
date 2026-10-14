namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal engagement milestone must pass. Check returns its problems, or none.</summary>
public interface ISeasonalEngagementMilestoneRule
{
    List<string> Check(SeasonalEngagementMilestone item);
}

public sealed record SeasonalEngagementMilestoneAllowedDescriptionRule(List<string> Allowed) : ISeasonalEngagementMilestoneRule
{
    public List<string> Check(SeasonalEngagementMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalEngagementMilestoneCodeLengthRule(int MaxLength) : ISeasonalEngagementMilestoneRule
{
    public List<string> Check(SeasonalEngagementMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalEngagementMilestoneRatingRangeRule(double Min, double Max) : ISeasonalEngagementMilestoneRule
{
    public List<string> Check(SeasonalEngagementMilestone item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalEngagementMilestoneRequiredCodeRule(string Message) : ISeasonalEngagementMilestoneRule
{
    public List<string> Check(SeasonalEngagementMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal engagement milestone.</summary>
public sealed record SeasonalEngagementMilestoneValidator(List<ISeasonalEngagementMilestoneRule> Rules)
{
    public List<string> Validate(SeasonalEngagementMilestone item)
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

    public void RequireValid(SeasonalEngagementMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalEngagementMilestone> items)
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

/// <summary>Functions of seasonal engagement milestone rules.</summary>
public static class SeasonalEngagementMilestoneRules
{
    public static SeasonalEngagementMilestoneValidator DefaultSeasonalEngagementMilestoneValidator()
    {
        return new SeasonalEngagementMilestoneValidator(
            new List<ISeasonalEngagementMilestoneRule>
            {
                new SeasonalEngagementMilestoneAllowedDescriptionRule(
                    new List<string> { "regional", "manual", "default" }),
                new SeasonalEngagementMilestoneCodeLengthRule(3600),
                new SeasonalEngagementMilestoneRatingRangeRule(8.0, 2407.9),
                new SeasonalEngagementMilestoneRequiredCodeRule("seasonal engagement milestone needs an entry"),
            });
    }
}
