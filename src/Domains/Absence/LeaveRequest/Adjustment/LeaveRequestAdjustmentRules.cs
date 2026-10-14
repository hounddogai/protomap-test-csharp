namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>A rule a leave request adjustment must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestAdjustmentRule
{
    List<string> Check(LeaveRequestAdjustment item);
}

public sealed record LeaveRequestAdjustmentTitleLengthRule(int MaxLength) : ILeaveRequestAdjustmentRule
{
    public List<string> Check(LeaveRequestAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestAdjustmentRequiredTitleRule(string Message) : ILeaveRequestAdjustmentRule
{
    public List<string> Check(LeaveRequestAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestAdjustmentGradeLengthRule(int MaxLength) : ILeaveRequestAdjustmentRule
{
    public List<string> Check(LeaveRequestAdjustment item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestAdjustmentCodeLengthRule(int MaxLength) : ILeaveRequestAdjustmentRule
{
    public List<string> Check(LeaveRequestAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request adjustment.</summary>
public sealed record LeaveRequestAdjustmentValidator(List<ILeaveRequestAdjustmentRule> Rules)
{
    public List<string> Validate(LeaveRequestAdjustment item)
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

    public void RequireValid(LeaveRequestAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestAdjustment> items)
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

/// <summary>Functions of leave request adjustment rules.</summary>
public static class LeaveRequestAdjustmentRules
{
    public static LeaveRequestAdjustmentValidator DefaultLeaveRequestAdjustmentValidator()
    {
        return new LeaveRequestAdjustmentValidator(
            new List<ILeaveRequestAdjustmentRule>
            {
                new LeaveRequestAdjustmentTitleLengthRule(32),
                new LeaveRequestAdjustmentRequiredTitleRule("leave request adjustment needs a value"),
                new LeaveRequestAdjustmentGradeLengthRule(86400),
                new LeaveRequestAdjustmentCodeLengthRule(32),
            });
    }
}
