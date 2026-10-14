namespace Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>A rule a action plan assignment must pass. Check returns its problems, or none.</summary>
public interface IActionPlanAssignmentRule
{
    List<string> Check(ActionPlanAssignment item);
}

public sealed record ActionPlanAssignmentRequiredSegmentRule(string Message) : IActionPlanAssignmentRule
{
    public List<string> Check(ActionPlanAssignment item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanAssignmentExpiresAtRangeRule(int Min, int Max) : IActionPlanAssignmentRule
{
    public List<string> Check(ActionPlanAssignment item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanAssignmentCodeLengthRule(int MaxLength) : IActionPlanAssignmentRule
{
    public List<string> Check(ActionPlanAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanAssignmentLocationLengthRule(int MaxLength) : IActionPlanAssignmentRule
{
    public List<string> Check(ActionPlanAssignment item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan assignment.</summary>
public sealed record ActionPlanAssignmentValidator(List<IActionPlanAssignmentRule> Rules)
{
    public List<string> Validate(ActionPlanAssignment item)
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

    public void RequireValid(ActionPlanAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanAssignment> items)
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

/// <summary>Functions of action plan assignment rules.</summary>
public static class ActionPlanAssignmentRules
{
    public static ActionPlanAssignmentValidator DefaultActionPlanAssignmentValidator()
    {
        return new ActionPlanAssignmentValidator(
            new List<IActionPlanAssignmentRule>
            {
                new ActionPlanAssignmentRequiredSegmentRule("action plan assignment needs this field"),
                new ActionPlanAssignmentExpiresAtRangeRule(16, 1000),
                new ActionPlanAssignmentCodeLengthRule(64),
                new ActionPlanAssignmentLocationLengthRule(80),
            });
    }
}
