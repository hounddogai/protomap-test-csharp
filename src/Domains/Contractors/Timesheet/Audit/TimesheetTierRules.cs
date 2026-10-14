namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet tier must pass. Check returns its problems, or none.</summary>
public interface ITimesheetTierRule
{
    List<string> Check(TimesheetTier item);
}

public sealed record TimesheetTierAllowedCostCenterRule(List<string> Allowed) : ITimesheetTierRule
{
    public List<string> Check(TimesheetTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetTierRequiredChecksumRule(string Message) : ITimesheetTierRule
{
    public List<string> Check(TimesheetTier item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetTierChecksumLengthRule(int MaxLength) : ITimesheetTierRule
{
    public List<string> Check(TimesheetTier item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet tier.</summary>
public sealed record TimesheetTierValidator(List<ITimesheetTierRule> Rules)
{
    public List<string> Validate(TimesheetTier item)
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

    public void RequireValid(TimesheetTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetTier> items)
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

/// <summary>Functions of timesheet tier rules.</summary>
public static class TimesheetTierRules
{
    public static TimesheetTierValidator DefaultTimesheetTierValidator()
    {
        return new TimesheetTierValidator(
            new List<ITimesheetTierRule>
            {
                new TimesheetTierAllowedCostCenterRule(new List<string> { "weighted" }),
                new TimesheetTierRequiredChecksumRule("timesheet tier needs a value"),
                new TimesheetTierChecksumLengthRule(1000),
            });
    }
}
