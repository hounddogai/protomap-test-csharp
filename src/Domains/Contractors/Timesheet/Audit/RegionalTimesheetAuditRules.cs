namespace Acme.Workforce.Contractors.Timesheet.Audit;

/// <summary>A rule a regional timesheet audit must pass. Check returns its problems, or none.</summary>
public interface IRegionalTimesheetAuditRule
{
    List<string> Check(RegionalTimesheetAudit item);
}

public sealed record RegionalTimesheetAuditApprovedGuardRule(double Limit) : IRegionalTimesheetAuditRule
{
    public List<string> Check(RegionalTimesheetAudit item)
    {
        if (item.Approved && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetAuditTitleLengthRule(int MaxLength) : IRegionalTimesheetAuditRule
{
    public List<string> Check(RegionalTimesheetAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetAuditRequiredTitleRule(string Message) : IRegionalTimesheetAuditRule
{
    public List<string> Check(RegionalTimesheetAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetAuditHoursRangeRule(double Min, double Max) : IRegionalTimesheetAuditRule
{
    public List<string> Check(RegionalTimesheetAudit item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional timesheet audit.</summary>
public sealed record RegionalTimesheetAuditValidator(List<IRegionalTimesheetAuditRule> Rules)
{
    public List<string> Validate(RegionalTimesheetAudit item)
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

    public void RequireValid(RegionalTimesheetAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalTimesheetAudit> items)
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

/// <summary>Functions of regional timesheet audit rules.</summary>
public static class RegionalTimesheetAuditRules
{
    public static RegionalTimesheetAuditValidator DefaultRegionalTimesheetAuditValidator()
    {
        return new RegionalTimesheetAuditValidator(
            new List<IRegionalTimesheetAuditRule>
            {
                new RegionalTimesheetAuditApprovedGuardRule(4.5),
                new RegionalTimesheetAuditTitleLengthRule(120),
                new RegionalTimesheetAuditRequiredTitleRule("regional timesheet audit needs an entry"),
                new RegionalTimesheetAuditHoursRangeRule(3.0, 3410.5),
            });
    }
}
