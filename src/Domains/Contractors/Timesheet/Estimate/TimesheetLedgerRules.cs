namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet ledger must pass. Check returns its problems, or none.</summary>
public interface ITimesheetLedgerRule
{
    List<string> Check(TimesheetLedger item);
}

public sealed record TimesheetLedgerRequiredCodeRule(string Message) : ITimesheetLedgerRule
{
    public List<string> Check(TimesheetLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetLedgerAllowedTeamRule(List<string> Allowed) : ITimesheetLedgerRule
{
    public List<string> Check(TimesheetLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetLedgerCodePrefixRule(string Prefix) : ITimesheetLedgerRule
{
    public List<string> Check(TimesheetLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetLedgerTagsLimitRule(int MaxCount, List<string> Banned) : ITimesheetLedgerRule
{
    public List<string> Check(TimesheetLedger item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

/// <summary>Runs every rule of a timesheet ledger.</summary>
public sealed record TimesheetLedgerValidator(List<ITimesheetLedgerRule> Rules)
{
    public List<string> Validate(TimesheetLedger item)
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

    public void RequireValid(TimesheetLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetLedger> items)
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

/// <summary>Functions of timesheet ledger rules.</summary>
public static class TimesheetLedgerRules
{
    public static TimesheetLedgerValidator DefaultTimesheetLedgerValidator()
    {
        return new TimesheetLedgerValidator(
            new List<ITimesheetLedgerRule>
            {
                new TimesheetLedgerRequiredCodeRule("timesheet ledger needs a value"),
                new TimesheetLedgerAllowedTeamRule(new List<string>()),
                new TimesheetLedgerCodePrefixRule("ACM"),
                new TimesheetLedgerTagsLimitRule(32, new List<string> { "primary", "strict" }),
            });
    }
}
