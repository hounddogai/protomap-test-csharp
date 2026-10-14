namespace Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>A rule a leave request audit must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestAuditRule
{
    List<string> Check(LeaveRequestAudit item);
}

public sealed record LeaveRequestAuditChecksumLengthRule(int MaxLength) : ILeaveRequestAuditRule
{
    public List<string> Check(LeaveRequestAudit item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestAuditFteRangeRule(double Min, double Max) : ILeaveRequestAuditRule
{
    public List<string> Check(LeaveRequestAudit item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LeaveRequestAuditTitleLengthRule(int MaxLength) : ILeaveRequestAuditRule
{
    public List<string> Check(LeaveRequestAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestAuditCodePrefixRule(string Prefix) : ILeaveRequestAuditRule
{
    public List<string> Check(LeaveRequestAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestAuditTeamLengthRule(int MaxLength) : ILeaveRequestAuditRule
{
    public List<string> Check(LeaveRequestAudit item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request audit.</summary>
public sealed record LeaveRequestAuditValidator(List<ILeaveRequestAuditRule> Rules)
{
    public List<string> Validate(LeaveRequestAudit item)
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

    public void RequireValid(LeaveRequestAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestAudit> items)
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

/// <summary>Functions of leave request audit rules.</summary>
public static class LeaveRequestAuditRules
{
    public static LeaveRequestAuditValidator DefaultLeaveRequestAuditValidator()
    {
        return new LeaveRequestAuditValidator(
            new List<ILeaveRequestAuditRule>
            {
                new LeaveRequestAuditChecksumLengthRule(80),
                new LeaveRequestAuditFteRangeRule(6.6, 4630.1),
                new LeaveRequestAuditTitleLengthRule(1000),
                new LeaveRequestAuditCodePrefixRule("ACM"),
                new LeaveRequestAuditTeamLengthRule(80),
            });
    }
}
