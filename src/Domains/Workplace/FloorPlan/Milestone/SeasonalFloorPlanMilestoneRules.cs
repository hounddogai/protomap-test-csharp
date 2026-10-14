namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal floor plan milestone must pass. Check returns its problems, or none.</summary>
public interface ISeasonalFloorPlanMilestoneRule
{
    List<string> Check(SeasonalFloorPlanMilestone item);
}

public sealed record SeasonalFloorPlanMilestoneRequiredCodeRule(string Message) : ISeasonalFloorPlanMilestoneRule
{
    public List<string> Check(SeasonalFloorPlanMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanMilestoneRequiredSourceRule(string Message) : ISeasonalFloorPlanMilestoneRule
{
    public List<string> Check(SeasonalFloorPlanMilestone item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanMilestoneCodeLengthRule(int MaxLength) : ISeasonalFloorPlanMilestoneRule
{
    public List<string> Check(SeasonalFloorPlanMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanMilestoneAllowedLabelRule(List<string> Allowed) : ISeasonalFloorPlanMilestoneRule
{
    public List<string> Check(SeasonalFloorPlanMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanMilestoneScoreRangeRule(double Min, double Max) : ISeasonalFloorPlanMilestoneRule
{
    public List<string> Check(SeasonalFloorPlanMilestone item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal floor plan milestone.</summary>
public sealed record SeasonalFloorPlanMilestoneValidator(List<ISeasonalFloorPlanMilestoneRule> Rules)
{
    public List<string> Validate(SeasonalFloorPlanMilestone item)
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

    public void RequireValid(SeasonalFloorPlanMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalFloorPlanMilestone> items)
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

/// <summary>Functions of seasonal floor plan milestone rules.</summary>
public static class SeasonalFloorPlanMilestoneRules
{
    public static SeasonalFloorPlanMilestoneValidator DefaultSeasonalFloorPlanMilestoneValidator()
    {
        return new SeasonalFloorPlanMilestoneValidator(
            new List<ISeasonalFloorPlanMilestoneRule>
            {
                new SeasonalFloorPlanMilestoneRequiredCodeRule("seasonal floor plan milestone needs this field"),
                new SeasonalFloorPlanMilestoneRequiredSourceRule("seasonal floor plan milestone needs an entry"),
                new SeasonalFloorPlanMilestoneCodeLengthRule(120),
                new SeasonalFloorPlanMilestoneAllowedLabelRule(new List<string> { "pending", "capped", "fallback" }),
                new SeasonalFloorPlanMilestoneScoreRangeRule(6.1, 1712.7),
            });
    }
}
