namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>A rule a timesheet override must pass. Check returns its problems, or none.</summary>
public interface ITimesheetOverrideRule
{
    List<string> Check(TimesheetOverride item);
}

public sealed record TimesheetOverrideGraceDaysRangeRule(int Min, int Max) : ITimesheetOverrideRule
{
    public List<string> Check(TimesheetOverride item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetOverrideRequiredRegionRule(string Message) : ITimesheetOverrideRule
{
    public List<string> Check(TimesheetOverride item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetOverrideRequiredCodeRule(string Message) : ITimesheetOverrideRule
{
    public List<string> Check(TimesheetOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetOverrideManagerIdLengthRule(int MaxLength) : ITimesheetOverrideRule
{
    public List<string> Check(TimesheetOverride item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetOverrideRequiredManagerIdRule(string Message) : ITimesheetOverrideRule
{
    public List<string> Check(TimesheetOverride item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetOverrideCodePrefixRule(string Prefix) : ITimesheetOverrideRule
{
    public List<string> Check(TimesheetOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet override.</summary>
public sealed record TimesheetOverrideValidator(List<ITimesheetOverrideRule> Rules)
{
    public List<string> Validate(TimesheetOverride item)
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

    public void RequireValid(TimesheetOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetOverride> items)
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

/// <summary>Functions of timesheet override rules.</summary>
public static class TimesheetOverrideRules
{
    public static TimesheetOverrideValidator DefaultTimesheetOverrideValidator()
    {
        return new TimesheetOverrideValidator(
            new List<ITimesheetOverrideRule>
            {
                new TimesheetOverrideGraceDaysRangeRule(120, 3600),
                new TimesheetOverrideRequiredRegionRule("timesheet override needs an entry"),
                new TimesheetOverrideRequiredCodeRule("timesheet override needs this field"),
                new TimesheetOverrideManagerIdLengthRule(120),
                new TimesheetOverrideRequiredManagerIdRule("timesheet override needs this field"),
                new TimesheetOverrideCodePrefixRule("Q"),
            });
    }
}
