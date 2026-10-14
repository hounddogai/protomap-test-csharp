namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

/// <summary>A rule a manual floor plan assignment must pass. Check returns its problems, or none.</summary>
public interface IManualFloorPlanAssignmentRule
{
    List<string> Check(ManualFloorPlanAssignment item);
}

public sealed record ManualFloorPlanAssignmentPriorityRangeRule(int Min, int Max) : IManualFloorPlanAssignmentRule
{
    public List<string> Check(ManualFloorPlanAssignment item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualFloorPlanAssignmentFloorValueRangeRule(double Min, double Max) : IManualFloorPlanAssignmentRule
{
    public List<string> Check(ManualFloorPlanAssignment item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualFloorPlanAssignmentReviewerLengthRule(int MaxLength) : IManualFloorPlanAssignmentRule
{
    public List<string> Check(ManualFloorPlanAssignment item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualFloorPlanAssignmentRequiredReviewerRule(string Message) : IManualFloorPlanAssignmentRule
{
    public List<string> Check(ManualFloorPlanAssignment item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual floor plan assignment.</summary>
public sealed record ManualFloorPlanAssignmentValidator(List<IManualFloorPlanAssignmentRule> Rules)
{
    public List<string> Validate(ManualFloorPlanAssignment item)
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

    public void RequireValid(ManualFloorPlanAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualFloorPlanAssignment> items)
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

/// <summary>Functions of manual floor plan assignment rules.</summary>
public static class ManualFloorPlanAssignmentRules
{
    public static ManualFloorPlanAssignmentValidator DefaultManualFloorPlanAssignmentValidator()
    {
        return new ManualFloorPlanAssignmentValidator(
            new List<IManualFloorPlanAssignmentRule>
            {
                new ManualFloorPlanAssignmentPriorityRangeRule(3600, 86400),
                new ManualFloorPlanAssignmentFloorValueRangeRule(7.1, 3265.5),
                new ManualFloorPlanAssignmentReviewerLengthRule(255),
                new ManualFloorPlanAssignmentRequiredReviewerRule("manual floor plan assignment needs this field"),
            });
    }
}
