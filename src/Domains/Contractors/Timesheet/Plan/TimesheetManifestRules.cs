namespace Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>A rule a timesheet manifest must pass. Check returns its problems, or none.</summary>
public interface ITimesheetManifestRule
{
    List<string> Check(TimesheetManifest item);
}

public sealed record TimesheetManifestRequiredCategoryRule(string Message) : ITimesheetManifestRule
{
    public List<string> Check(TimesheetManifest item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetManifestRequiredCodeRule(string Message) : ITimesheetManifestRule
{
    public List<string> Check(TimesheetManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetManifestRequiredBadgeRule(string Message) : ITimesheetManifestRule
{
    public List<string> Check(TimesheetManifest item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetManifestMaximumRangeRule(double Min, double Max) : ITimesheetManifestRule
{
    public List<string> Check(TimesheetManifest item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetManifestCategoryLengthRule(int MaxLength) : ITimesheetManifestRule
{
    public List<string> Check(TimesheetManifest item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetManifestTitleLengthRule(int MaxLength) : ITimesheetManifestRule
{
    public List<string> Check(TimesheetManifest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet manifest.</summary>
public sealed record TimesheetManifestValidator(List<ITimesheetManifestRule> Rules)
{
    public List<string> Validate(TimesheetManifest item)
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

    public void RequireValid(TimesheetManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetManifest> items)
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

/// <summary>Functions of timesheet manifest rules.</summary>
public static class TimesheetManifestRules
{
    public static TimesheetManifestValidator DefaultTimesheetManifestValidator()
    {
        return new TimesheetManifestValidator(
            new List<ITimesheetManifestRule>
            {
                new TimesheetManifestRequiredCategoryRule("timesheet manifest needs a value"),
                new TimesheetManifestRequiredCodeRule("timesheet manifest needs this field"),
                new TimesheetManifestRequiredBadgeRule("timesheet manifest needs a value"),
                new TimesheetManifestMaximumRangeRule(7.2, 420.0),
                new TimesheetManifestCategoryLengthRule(86400),
                new TimesheetManifestTitleLengthRule(3600),
            });
    }
}
