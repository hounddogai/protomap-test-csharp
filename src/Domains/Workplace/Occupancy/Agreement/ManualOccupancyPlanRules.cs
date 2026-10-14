namespace Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>A rule a manual occupancy plan must pass. Check returns its problems, or none.</summary>
public interface IManualOccupancyPlanRule
{
    List<string> Check(ManualOccupancyPlan item);
}

public sealed record ManualOccupancyPlanEndsAtRangeRule(int Min, int Max) : IManualOccupancyPlanRule
{
    public List<string> Check(ManualOccupancyPlan item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualOccupancyPlanCodeLengthRule(int MaxLength) : IManualOccupancyPlanRule
{
    public List<string> Check(ManualOccupancyPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualOccupancyPlanRatingRangeRule(double Min, double Max) : IManualOccupancyPlanRule
{
    public List<string> Check(ManualOccupancyPlan item)
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

public sealed record ManualOccupancyPlanRequiredSegmentRule(string Message) : IManualOccupancyPlanRule
{
    public List<string> Check(ManualOccupancyPlan item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualOccupancyPlanSkillLengthRule(int MaxLength) : IManualOccupancyPlanRule
{
    public List<string> Check(ManualOccupancyPlan item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual occupancy plan.</summary>
public sealed record ManualOccupancyPlanValidator(List<IManualOccupancyPlanRule> Rules)
{
    public List<string> Validate(ManualOccupancyPlan item)
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

    public void RequireValid(ManualOccupancyPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualOccupancyPlan> items)
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

/// <summary>Functions of manual occupancy plan rules.</summary>
public static class ManualOccupancyPlanRules
{
    public static ManualOccupancyPlanValidator DefaultManualOccupancyPlanValidator()
    {
        return new ManualOccupancyPlanValidator(
            new List<IManualOccupancyPlanRule>
            {
                new ManualOccupancyPlanEndsAtRangeRule(255, 86400),
                new ManualOccupancyPlanCodeLengthRule(86400),
                new ManualOccupancyPlanRatingRangeRule(9.2, 921.2),
                new ManualOccupancyPlanRequiredSegmentRule("manual occupancy plan needs this field"),
                new ManualOccupancyPlanSkillLengthRule(3600),
            });
    }
}
