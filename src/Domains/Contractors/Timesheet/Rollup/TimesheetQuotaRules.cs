namespace Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>A rule a timesheet quota must pass. Check returns its problems, or none.</summary>
public interface ITimesheetQuotaRule
{
    List<string> Check(TimesheetQuota item);
}

public sealed record TimesheetQuotaRequiredLabelRule(string Message) : ITimesheetQuotaRule
{
    public List<string> Check(TimesheetQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetQuotaCodePrefixRule(string Prefix) : ITimesheetQuotaRule
{
    public List<string> Check(TimesheetQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetQuotaBadgeLengthRule(int MaxLength) : ITimesheetQuotaRule
{
    public List<string> Check(TimesheetQuota item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetQuotaSkillLengthRule(int MaxLength) : ITimesheetQuotaRule
{
    public List<string> Check(TimesheetQuota item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet quota.</summary>
public sealed record TimesheetQuotaValidator(List<ITimesheetQuotaRule> Rules)
{
    public List<string> Validate(TimesheetQuota item)
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

    public void RequireValid(TimesheetQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetQuota> items)
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

/// <summary>Functions of timesheet quota rules.</summary>
public static class TimesheetQuotaRules
{
    public static TimesheetQuotaValidator DefaultTimesheetQuotaValidator()
    {
        return new TimesheetQuotaValidator(
            new List<ITimesheetQuotaRule>
            {
                new TimesheetQuotaRequiredLabelRule("timesheet quota needs a value"),
                new TimesheetQuotaCodePrefixRule("ACM"),
                new TimesheetQuotaBadgeLengthRule(16),
                new TimesheetQuotaSkillLengthRule(3600),
            });
    }
}
