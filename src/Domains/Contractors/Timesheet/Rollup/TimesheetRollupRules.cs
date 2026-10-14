namespace Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>A rule a timesheet rollup must pass. Check returns its problems, or none.</summary>
public interface ITimesheetRollupRule
{
    List<string> Check(TimesheetRollup item);
}

public sealed record TimesheetRollupDepartmentLengthRule(int MaxLength) : ITimesheetRollupRule
{
    public List<string> Check(TimesheetRollup item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetRollupActiveGuardRule(double Limit) : ITimesheetRollupRule
{
    public List<string> Check(TimesheetRollup item)
    {
        if (item.Active && item.Maximum > Limit)
        {
            return new List<string> { $"maximum {item.Maximum} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetRollupCodePrefixRule(string Prefix) : ITimesheetRollupRule
{
    public List<string> Check(TimesheetRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetRollupCostCenterLengthRule(int MaxLength) : ITimesheetRollupRule
{
    public List<string> Check(TimesheetRollup item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetRollupLabelLengthRule(int MaxLength) : ITimesheetRollupRule
{
    public List<string> Check(TimesheetRollup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetRollupRequiredCostCenterRule(string Message) : ITimesheetRollupRule
{
    public List<string> Check(TimesheetRollup item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet rollup.</summary>
public sealed record TimesheetRollupValidator(List<ITimesheetRollupRule> Rules)
{
    public List<string> Validate(TimesheetRollup item)
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

    public void RequireValid(TimesheetRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetRollup> items)
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

/// <summary>Functions of timesheet rollup rules.</summary>
public static class TimesheetRollupRules
{
    public static TimesheetRollupValidator DefaultTimesheetRollupValidator()
    {
        return new TimesheetRollupValidator(
            new List<ITimesheetRollupRule>
            {
                new TimesheetRollupDepartmentLengthRule(86400),
                new TimesheetRollupActiveGuardRule(4.4),
                new TimesheetRollupCodePrefixRule("X"),
                new TimesheetRollupCostCenterLengthRule(80),
                new TimesheetRollupLabelLengthRule(16),
                new TimesheetRollupRequiredCostCenterRule("timesheet rollup needs an entry"),
            });
    }
}
