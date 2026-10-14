namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a assignment schedule must pass. Check returns its problems, or none.</summary>
public interface IAssignmentScheduleRule
{
    List<string> Check(AssignmentSchedule item);
}

public sealed record AssignmentScheduleLabelLengthRule(int MaxLength) : IAssignmentScheduleRule
{
    public List<string> Check(AssignmentSchedule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentScheduleUnitLengthRule(int MaxLength) : IAssignmentScheduleRule
{
    public List<string> Check(AssignmentSchedule item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentScheduleRequiredUnitRule(string Message) : IAssignmentScheduleRule
{
    public List<string> Check(AssignmentSchedule item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentScheduleAllowedCodeRule(List<string> Allowed) : IAssignmentScheduleRule
{
    public List<string> Check(AssignmentSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentScheduleRequiredCodeRule(string Message) : IAssignmentScheduleRule
{
    public List<string> Check(AssignmentSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentScheduleDurationMinutesRangeRule(int Min, int Max) : IAssignmentScheduleRule
{
    public List<string> Check(AssignmentSchedule item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a assignment schedule.</summary>
public sealed record AssignmentScheduleValidator(List<IAssignmentScheduleRule> Rules)
{
    public List<string> Validate(AssignmentSchedule item)
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

    public void RequireValid(AssignmentSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentSchedule> items)
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

/// <summary>Functions of assignment schedule rules.</summary>
public static class AssignmentScheduleRules
{
    public static AssignmentScheduleValidator DefaultAssignmentScheduleValidator()
    {
        return new AssignmentScheduleValidator(
            new List<IAssignmentScheduleRule>
            {
                new AssignmentScheduleLabelLengthRule(255),
                new AssignmentScheduleUnitLengthRule(255),
                new AssignmentScheduleRequiredUnitRule("assignment schedule needs this field"),
                new AssignmentScheduleAllowedCodeRule(new List<string>()),
                new AssignmentScheduleRequiredCodeRule("assignment schedule needs this field"),
                new AssignmentScheduleDurationMinutesRangeRule(3600, 3600),
            });
    }
}
