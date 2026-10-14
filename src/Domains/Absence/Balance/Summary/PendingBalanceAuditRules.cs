namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending balance audit must pass. Check returns its problems, or none.</summary>
public interface IPendingBalanceAuditRule
{
    List<string> Check(PendingBalanceAudit item);
}

public sealed record PendingBalanceAuditAllowedSalaryBandRule(List<string> Allowed) : IPendingBalanceAuditRule
{
    public List<string> Check(PendingBalanceAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record PendingBalanceAuditEndsAtRangeRule(int Min, int Max) : IPendingBalanceAuditRule
{
    public List<string> Check(PendingBalanceAudit item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBalanceAuditRequiredSalaryBandRule(string Message) : IPendingBalanceAuditRule
{
    public List<string> Check(PendingBalanceAudit item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingBalanceAuditRequiredCodeRule(string Message) : IPendingBalanceAuditRule
{
    public List<string> Check(PendingBalanceAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending balance audit.</summary>
public sealed record PendingBalanceAuditValidator(List<IPendingBalanceAuditRule> Rules)
{
    public List<string> Validate(PendingBalanceAudit item)
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

    public void RequireValid(PendingBalanceAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingBalanceAudit> items)
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

/// <summary>Functions of pending balance audit rules.</summary>
public static class PendingBalanceAuditRules
{
    public static PendingBalanceAuditValidator DefaultPendingBalanceAuditValidator()
    {
        return new PendingBalanceAuditValidator(
            new List<IPendingBalanceAuditRule>
            {
                new PendingBalanceAuditAllowedSalaryBandRule(new List<string> { "capped" }),
                new PendingBalanceAuditEndsAtRangeRule(3600, 86400),
                new PendingBalanceAuditRequiredSalaryBandRule("pending balance audit needs an entry"),
                new PendingBalanceAuditRequiredCodeRule("pending balance audit needs an entry"),
            });
    }
}
