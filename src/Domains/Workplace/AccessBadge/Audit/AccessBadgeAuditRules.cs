namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge audit must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeAuditRule
{
    List<string> Check(AccessBadgeAudit item);
}

public sealed record AccessBadgeAuditSkillLengthRule(int MaxLength) : IAccessBadgeAuditRule
{
    public List<string> Check(AccessBadgeAudit item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAuditRetriesRangeRule(int Min, int Max) : IAccessBadgeAuditRule
{
    public List<string> Check(AccessBadgeAudit item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAuditAllowedEmployeeIdRule(List<string> Allowed) : IAccessBadgeAuditRule
{
    public List<string> Check(AccessBadgeAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAuditQuantityRangeRule(int Min, int Max) : IAccessBadgeAuditRule
{
    public List<string> Check(AccessBadgeAudit item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccessBadgeAuditCodeLengthRule(int MaxLength) : IAccessBadgeAuditRule
{
    public List<string> Check(AccessBadgeAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAuditNameLengthRule(int MaxLength) : IAccessBadgeAuditRule
{
    public List<string> Check(AccessBadgeAudit item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge audit.</summary>
public sealed record AccessBadgeAuditValidator(List<IAccessBadgeAuditRule> Rules)
{
    public List<string> Validate(AccessBadgeAudit item)
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

    public void RequireValid(AccessBadgeAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeAudit> items)
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

/// <summary>Functions of access badge audit rules.</summary>
public static class AccessBadgeAuditRules
{
    public static AccessBadgeAuditValidator DefaultAccessBadgeAuditValidator()
    {
        return new AccessBadgeAuditValidator(
            new List<IAccessBadgeAuditRule>
            {
                new AccessBadgeAuditSkillLengthRule(32),
                new AccessBadgeAuditRetriesRangeRule(32, 1000),
                new AccessBadgeAuditAllowedEmployeeIdRule(new List<string> { "primary" }),
                new AccessBadgeAuditQuantityRangeRule(16, 80),
                new AccessBadgeAuditCodeLengthRule(80),
                new AccessBadgeAuditNameLengthRule(255),
            });
    }
}
