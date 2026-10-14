namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy leave request audit must pass. Check returns its problems, or none.</summary>
public interface ILegacyLeaveRequestAuditRule
{
    List<string> Check(LegacyLeaveRequestAudit item);
}

public sealed record LegacyLeaveRequestAuditRetriesRangeRule(int Min, int Max) : ILegacyLeaveRequestAuditRule
{
    public List<string> Check(LegacyLeaveRequestAudit item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLeaveRequestAuditRequiredCodeRule(string Message) : ILegacyLeaveRequestAuditRule
{
    public List<string> Check(LegacyLeaveRequestAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyLeaveRequestAuditPinnedGuardRule(int Limit) : ILegacyLeaveRequestAuditRule
{
    public List<string> Check(LegacyLeaveRequestAudit item)
    {
        if (item.Pinned && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLeaveRequestAuditAllowedCodeRule(List<string> Allowed) : ILegacyLeaveRequestAuditRule
{
    public List<string> Check(LegacyLeaveRequestAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy leave request audit.</summary>
public sealed record LegacyLeaveRequestAuditValidator(List<ILegacyLeaveRequestAuditRule> Rules)
{
    public List<string> Validate(LegacyLeaveRequestAudit item)
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

    public void RequireValid(LegacyLeaveRequestAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyLeaveRequestAudit> items)
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

/// <summary>Functions of legacy leave request audit rules.</summary>
public static class LegacyLeaveRequestAuditRules
{
    public static LegacyLeaveRequestAuditValidator DefaultLegacyLeaveRequestAuditValidator()
    {
        return new LegacyLeaveRequestAuditValidator(
            new List<ILegacyLeaveRequestAuditRule>
            {
                new LegacyLeaveRequestAuditRetriesRangeRule(64, 80),
                new LegacyLeaveRequestAuditRequiredCodeRule("legacy leave request audit needs this field"),
                new LegacyLeaveRequestAuditPinnedGuardRule(120),
                new LegacyLeaveRequestAuditAllowedCodeRule(new List<string> { "strict", "rolling", "primary" }),
            });
    }
}
