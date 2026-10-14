namespace Acme.Workforce.Assets.Assignment.Exception;

/// <summary>A rule a assignment group must pass. Check returns its problems, or none.</summary>
public interface IAssignmentGroupRule
{
    List<string> Check(AssignmentGroup item);
}

public sealed record AssignmentGroupDescriptionLengthRule(int MaxLength) : IAssignmentGroupRule
{
    public List<string> Check(AssignmentGroup item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentGroupDurationMinutesRangeRule(int Min, int Max) : IAssignmentGroupRule
{
    public List<string> Check(AssignmentGroup item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentGroupRequiredCodeRule(string Message) : IAssignmentGroupRule
{
    public List<string> Check(AssignmentGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment group.</summary>
public sealed record AssignmentGroupValidator(List<IAssignmentGroupRule> Rules)
{
    public List<string> Validate(AssignmentGroup item)
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

    public void RequireValid(AssignmentGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentGroup> items)
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

/// <summary>Functions of assignment group rules.</summary>
public static class AssignmentGroupRules
{
    public static AssignmentGroupValidator DefaultAssignmentGroupValidator()
    {
        return new AssignmentGroupValidator(
            new List<IAssignmentGroupRule>
            {
                new AssignmentGroupDescriptionLengthRule(32),
                new AssignmentGroupDurationMinutesRangeRule(16, 16),
                new AssignmentGroupRequiredCodeRule("assignment group needs an entry"),
            });
    }
}
