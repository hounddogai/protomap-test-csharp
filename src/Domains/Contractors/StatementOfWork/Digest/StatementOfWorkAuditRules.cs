namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a statement of work audit must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkAuditRule
{
    List<string> Check(StatementOfWorkAudit item);
}

public sealed record StatementOfWorkAuditPinnedGuardRule(int Limit) : IStatementOfWorkAuditRule
{
    public List<string> Check(StatementOfWorkAudit item)
    {
        if (item.Pinned && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkAuditAllowedCodeRule(List<string> Allowed) : IStatementOfWorkAuditRule
{
    public List<string> Check(StatementOfWorkAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkAuditEmployeeIdLengthRule(int MaxLength) : IStatementOfWorkAuditRule
{
    public List<string> Check(StatementOfWorkAudit item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work audit.</summary>
public sealed record StatementOfWorkAuditValidator(List<IStatementOfWorkAuditRule> Rules)
{
    public List<string> Validate(StatementOfWorkAudit item)
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

    public void RequireValid(StatementOfWorkAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkAudit> items)
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

/// <summary>Functions of statement of work audit rules.</summary>
public static class StatementOfWorkAuditRules
{
    public static StatementOfWorkAuditValidator DefaultStatementOfWorkAuditValidator()
    {
        return new StatementOfWorkAuditValidator(
            new List<IStatementOfWorkAuditRule>
            {
                new StatementOfWorkAuditPinnedGuardRule(32),
                new StatementOfWorkAuditAllowedCodeRule(new List<string> { "scheduled", "scheduled" }),
                new StatementOfWorkAuditEmployeeIdLengthRule(255),
            });
    }
}
