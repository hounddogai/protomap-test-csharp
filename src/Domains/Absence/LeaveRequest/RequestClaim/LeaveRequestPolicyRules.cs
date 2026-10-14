namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>A rule a leave request policy must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestPolicyRule
{
    List<string> Check(LeaveRequestPolicy item);
}

public sealed record LeaveRequestPolicyLabelLengthRule(int MaxLength) : ILeaveRequestPolicyRule
{
    public List<string> Check(LeaveRequestPolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestPolicyRequiredCodeRule(string Message) : ILeaveRequestPolicyRule
{
    public List<string> Check(LeaveRequestPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestPolicyNotesLengthRule(int MaxLength) : ILeaveRequestPolicyRule
{
    public List<string> Check(LeaveRequestPolicy item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request policy.</summary>
public sealed record LeaveRequestPolicyValidator(List<ILeaveRequestPolicyRule> Rules)
{
    public List<string> Validate(LeaveRequestPolicy item)
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

    public void RequireValid(LeaveRequestPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestPolicy> items)
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

/// <summary>Functions of leave request policy rules.</summary>
public static class LeaveRequestPolicyRules
{
    public static LeaveRequestPolicyValidator DefaultLeaveRequestPolicyValidator()
    {
        return new LeaveRequestPolicyValidator(
            new List<ILeaveRequestPolicyRule>
            {
                new LeaveRequestPolicyLabelLengthRule(64),
                new LeaveRequestPolicyRequiredCodeRule("leave request policy needs this field"),
                new LeaveRequestPolicyNotesLengthRule(16),
            });
    }
}
