namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance audit must pass. Check returns its problems, or none.</summary>
public interface IBalanceAuditRule
{
    List<string> Check(BalanceAudit item);
}

public sealed record BalanceAuditRequiredTitleRule(string Message) : IBalanceAuditRule
{
    public List<string> Check(BalanceAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceAuditAllowedCodeRule(List<string> Allowed) : IBalanceAuditRule
{
    public List<string> Check(BalanceAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record BalanceAuditNotesLengthRule(int MaxLength) : IBalanceAuditRule
{
    public List<string> Check(BalanceAudit item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance audit.</summary>
public sealed record BalanceAuditValidator(List<IBalanceAuditRule> Rules)
{
    public List<string> Validate(BalanceAudit item)
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

    public void RequireValid(BalanceAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceAudit> items)
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

/// <summary>Functions of balance audit rules.</summary>
public static class BalanceAuditRules
{
    public static BalanceAuditValidator DefaultBalanceAuditValidator()
    {
        return new BalanceAuditValidator(
            new List<IBalanceAuditRule>
            {
                new BalanceAuditRequiredTitleRule("balance audit needs a value"),
                new BalanceAuditAllowedCodeRule(new List<string>()),
                new BalanceAuditNotesLengthRule(3600),
            });
    }
}
