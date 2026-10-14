namespace Acme.Workforce.Compliance.Audit.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional audit record must pass. Check returns its problems, or none.</summary>
public interface IRegionalAuditRecordRule
{
    List<string> Check(RegionalAuditRecord item);
}

public sealed record RegionalAuditRecordPriorityRangeRule(int Min, int Max) : IRegionalAuditRecordRule
{
    public List<string> Check(RegionalAuditRecord item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAuditRecordAllowedBadgeRule(List<string> Allowed) : IRegionalAuditRecordRule
{
    public List<string> Check(RegionalAuditRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAuditRecordLabelLengthRule(int MaxLength) : IRegionalAuditRecordRule
{
    public List<string> Check(RegionalAuditRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional audit record.</summary>
public sealed record RegionalAuditRecordValidator(List<IRegionalAuditRecordRule> Rules)
{
    public List<string> Validate(RegionalAuditRecord item)
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

    public void RequireValid(RegionalAuditRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAuditRecord> items)
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

/// <summary>Functions of regional audit record rules.</summary>
public static class RegionalAuditRecordRules
{
    public static RegionalAuditRecordValidator DefaultRegionalAuditRecordValidator()
    {
        return new RegionalAuditRecordValidator(
            new List<IRegionalAuditRecordRule>
            {
                new RegionalAuditRecordPriorityRangeRule(255, 1000),
                new RegionalAuditRecordAllowedBadgeRule(new List<string> { "weighted", "manual", "regional" }),
                new RegionalAuditRecordLabelLengthRule(80),
            });
    }
}
