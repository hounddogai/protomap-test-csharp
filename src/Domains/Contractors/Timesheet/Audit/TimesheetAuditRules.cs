namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet audit must pass. Check returns its problems, or none.</summary>
public interface ITimesheetAuditRule
{
    List<string> Check(TimesheetAudit item);
}

public sealed record TimesheetAuditCodePrefixRule(string Prefix) : ITimesheetAuditRule
{
    public List<string> Check(TimesheetAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAuditTenureMonthsRangeRule(int Min, int Max) : ITimesheetAuditRule
{
    public List<string> Check(TimesheetAudit item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAuditRequiredCodeRule(string Message) : ITimesheetAuditRule
{
    public List<string> Check(TimesheetAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAuditLabelLengthRule(int MaxLength) : ITimesheetAuditRule
{
    public List<string> Check(TimesheetAudit item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAuditAllowedBadgeRule(List<string> Allowed) : ITimesheetAuditRule
{
    public List<string> Check(TimesheetAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is too long" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAuditRateRangeRule(double Min, double Max) : ITimesheetAuditRule
{
    public List<string> Check(TimesheetAudit item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet audit.</summary>
public sealed record TimesheetAuditValidator(List<ITimesheetAuditRule> Rules)
{
    public List<string> Validate(TimesheetAudit item)
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

    public void RequireValid(TimesheetAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetAudit> items)
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

/// <summary>Functions of timesheet audit rules.</summary>
public static class TimesheetAuditRules
{
    public static TimesheetAuditValidator DefaultTimesheetAuditValidator()
    {
        return new TimesheetAuditValidator(
            new List<ITimesheetAuditRule>
            {
                new TimesheetAuditCodePrefixRule("REF"),
                new TimesheetAuditTenureMonthsRangeRule(255, 255),
                new TimesheetAuditRequiredCodeRule("timesheet audit needs an entry"),
                new TimesheetAuditLabelLengthRule(86400),
                new TimesheetAuditAllowedBadgeRule(new List<string> { "strict", "capped", "default" }),
                new TimesheetAuditRateRangeRule(8.4, 4357.2),
            });
    }
}
