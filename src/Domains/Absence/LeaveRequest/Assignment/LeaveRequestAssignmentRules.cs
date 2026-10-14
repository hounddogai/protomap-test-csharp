namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request assignment must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestAssignmentRule
{
    List<string> Check(LeaveRequestAssignment item);
}

public sealed record LeaveRequestAssignmentLeaveDaysRangeRule(int Min, int Max) : ILeaveRequestAssignmentRule
{
    public List<string> Check(LeaveRequestAssignment item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestAssignmentAllowedCodeRule(List<string> Allowed) : ILeaveRequestAssignmentRule
{
    public List<string> Check(LeaveRequestAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestAssignmentRevisionRangeRule(int Min, int Max) : ILeaveRequestAssignmentRule
{
    public List<string> Check(LeaveRequestAssignment item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LeaveRequestAssignmentCodeLengthRule(int MaxLength) : ILeaveRequestAssignmentRule
{
    public List<string> Check(LeaveRequestAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request assignment.</summary>
public sealed record LeaveRequestAssignmentValidator(List<ILeaveRequestAssignmentRule> Rules)
{
    public List<string> Validate(LeaveRequestAssignment item)
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

    public void RequireValid(LeaveRequestAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestAssignment> items)
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

/// <summary>Functions of leave request assignment rules.</summary>
public static class LeaveRequestAssignmentRules
{
    public static LeaveRequestAssignmentValidator DefaultLeaveRequestAssignmentValidator()
    {
        return new LeaveRequestAssignmentValidator(
            new List<ILeaveRequestAssignmentRule>
            {
                new LeaveRequestAssignmentLeaveDaysRangeRule(255, 1000),
                new LeaveRequestAssignmentAllowedCodeRule(new List<string> { "regional", "legacy" }),
                new LeaveRequestAssignmentRevisionRangeRule(16, 64),
                new LeaveRequestAssignmentCodeLengthRule(86400),
            });
    }
}
