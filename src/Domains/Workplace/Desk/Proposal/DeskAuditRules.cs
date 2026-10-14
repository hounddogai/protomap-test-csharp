namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a desk audit must pass. Check returns its problems, or none.</summary>
public interface IDeskAuditRule
{
    List<string> Check(DeskAudit item);
}

public sealed record DeskAuditCodePrefixRule(string Prefix) : IDeskAuditRule
{
    public List<string> Check(DeskAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskAuditRequiredUnitRule(string Message) : IDeskAuditRule
{
    public List<string> Check(DeskAudit item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskAuditDurationMinutesRangeRule(int Min, int Max) : IDeskAuditRule
{
    public List<string> Check(DeskAudit item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeskAuditAllowedManagerIdRule(List<string> Allowed) : IDeskAuditRule
{
    public List<string> Check(DeskAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is expired" };
        }
        return new List<string>();
    }
}

public sealed record DeskAuditRequiredLabelRule(string Message) : IDeskAuditRule
{
    public List<string> Check(DeskAudit item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk audit.</summary>
public sealed record DeskAuditValidator(List<IDeskAuditRule> Rules)
{
    public List<string> Validate(DeskAudit item)
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

    public void RequireValid(DeskAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskAudit> items)
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

/// <summary>Functions of desk audit rules.</summary>
public static class DeskAuditRules
{
    public static DeskAuditValidator DefaultDeskAuditValidator()
    {
        return new DeskAuditValidator(
            new List<IDeskAuditRule>
            {
                new DeskAuditCodePrefixRule("X"),
                new DeskAuditRequiredUnitRule("desk audit needs this field"),
                new DeskAuditDurationMinutesRangeRule(120, 1000),
                new DeskAuditAllowedManagerIdRule(new List<string> { "weighted", "manual", "batched" }),
                new DeskAuditRequiredLabelRule("desk audit needs an entry"),
            });
    }
}
