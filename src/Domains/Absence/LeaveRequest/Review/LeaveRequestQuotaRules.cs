namespace Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>A rule a leave request quota must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestQuotaRule
{
    List<string> Check(LeaveRequestQuota item);
}

public sealed record LeaveRequestQuotaRequiredGradeRule(string Message) : ILeaveRequestQuotaRule
{
    public List<string> Check(LeaveRequestQuota item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestQuotaNameLengthRule(int MaxLength) : ILeaveRequestQuotaRule
{
    public List<string> Check(LeaveRequestQuota item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestQuotaRequiredNameRule(string Message) : ILeaveRequestQuotaRule
{
    public List<string> Check(LeaveRequestQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request quota.</summary>
public sealed record LeaveRequestQuotaValidator(List<ILeaveRequestQuotaRule> Rules)
{
    public List<string> Validate(LeaveRequestQuota item)
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

    public void RequireValid(LeaveRequestQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestQuota> items)
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

/// <summary>Functions of leave request quota rules.</summary>
public static class LeaveRequestQuotaRules
{
    public static LeaveRequestQuotaValidator DefaultLeaveRequestQuotaValidator()
    {
        return new LeaveRequestQuotaValidator(
            new List<ILeaveRequestQuotaRule>
            {
                new LeaveRequestQuotaRequiredGradeRule("leave request quota needs this field"),
                new LeaveRequestQuotaNameLengthRule(16),
                new LeaveRequestQuotaRequiredNameRule("leave request quota needs this field"),
            });
    }
}
