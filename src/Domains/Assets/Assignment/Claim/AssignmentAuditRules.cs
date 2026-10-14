namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a assignment audit must pass. Check returns its problems, or none.</summary>
public interface IAssignmentAuditRule
{
    List<string> Check(AssignmentAudit item);
}

public sealed record AssignmentAuditRequiredTeamRule(string Message) : IAssignmentAuditRule
{
    public List<string> Check(AssignmentAudit item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentAuditCodeLengthRule(int MaxLength) : IAssignmentAuditRule
{
    public List<string> Check(AssignmentAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentAuditAllowedManagerIdRule(List<string> Allowed) : IAssignmentAuditRule
{
    public List<string> Check(AssignmentAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment audit.</summary>
public sealed record AssignmentAuditValidator(List<IAssignmentAuditRule> Rules)
{
    public List<string> Validate(AssignmentAudit item)
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

    public void RequireValid(AssignmentAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentAudit> items)
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

/// <summary>Functions of assignment audit rules.</summary>
public static class AssignmentAuditRules
{
    public static AssignmentAuditValidator DefaultAssignmentAuditValidator()
    {
        return new AssignmentAuditValidator(
            new List<IAssignmentAuditRule>
            {
                new AssignmentAuditRequiredTeamRule("assignment audit needs an entry"),
                new AssignmentAuditCodeLengthRule(16),
                new AssignmentAuditAllowedManagerIdRule(new List<string> { "lenient", "primary", "seasonal" }),
            });
    }
}
