namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>A rule a strict access badge audit must pass. Check returns its problems, or none.</summary>
public interface IStrictAccessBadgeAuditRule
{
    List<string> Check(StrictAccessBadgeAudit item);
}

public sealed record StrictAccessBadgeAuditLabelLengthRule(int MaxLength) : IStrictAccessBadgeAuditRule
{
    public List<string> Check(StrictAccessBadgeAudit item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAccessBadgeAuditRequiredDescriptionRule(string Message) : IStrictAccessBadgeAuditRule
{
    public List<string> Check(StrictAccessBadgeAudit item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAccessBadgeAuditRequiredLabelRule(string Message) : IStrictAccessBadgeAuditRule
{
    public List<string> Check(StrictAccessBadgeAudit item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict access badge audit.</summary>
public sealed record StrictAccessBadgeAuditValidator(List<IStrictAccessBadgeAuditRule> Rules)
{
    public List<string> Validate(StrictAccessBadgeAudit item)
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

    public void RequireValid(StrictAccessBadgeAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAccessBadgeAudit> items)
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

/// <summary>Functions of strict access badge audit rules.</summary>
public static class StrictAccessBadgeAuditRules
{
    public static StrictAccessBadgeAuditValidator DefaultStrictAccessBadgeAuditValidator()
    {
        return new StrictAccessBadgeAuditValidator(
            new List<IStrictAccessBadgeAuditRule>
            {
                new StrictAccessBadgeAuditLabelLengthRule(86400),
                new StrictAccessBadgeAuditRequiredDescriptionRule("strict access badge audit needs a value"),
                new StrictAccessBadgeAuditRequiredLabelRule("strict access badge audit needs a value"),
            });
    }
}
