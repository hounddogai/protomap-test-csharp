namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet bundle must pass. Check returns its problems, or none.</summary>
public interface ITimesheetBundleRule
{
    List<string> Check(TimesheetBundle item);
}

public sealed record TimesheetBundleRequiredJobCodeRule(string Message) : ITimesheetBundleRule
{
    public List<string> Check(TimesheetBundle item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBundleRequiredCodeRule(string Message) : ITimesheetBundleRule
{
    public List<string> Check(TimesheetBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBundleCodePrefixRule(string Prefix) : ITimesheetBundleRule
{
    public List<string> Check(TimesheetBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBundleCategoryLengthRule(int MaxLength) : ITimesheetBundleRule
{
    public List<string> Check(TimesheetBundle item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBundleGraceDaysRangeRule(int Min, int Max) : ITimesheetBundleRule
{
    public List<string> Check(TimesheetBundle item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBundleAllowedCategoryRule(List<string> Allowed) : ITimesheetBundleRule
{
    public List<string> Check(TimesheetBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet bundle.</summary>
public sealed record TimesheetBundleValidator(List<ITimesheetBundleRule> Rules)
{
    public List<string> Validate(TimesheetBundle item)
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

    public void RequireValid(TimesheetBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetBundle> items)
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

/// <summary>Functions of timesheet bundle rules.</summary>
public static class TimesheetBundleRules
{
    public static TimesheetBundleValidator DefaultTimesheetBundleValidator()
    {
        return new TimesheetBundleValidator(
            new List<ITimesheetBundleRule>
            {
                new TimesheetBundleRequiredJobCodeRule("timesheet bundle needs this field"),
                new TimesheetBundleRequiredCodeRule("timesheet bundle needs an entry"),
                new TimesheetBundleCodePrefixRule("ACM"),
                new TimesheetBundleCategoryLengthRule(3600),
                new TimesheetBundleGraceDaysRangeRule(255, 1000),
                new TimesheetBundleAllowedCategoryRule(new List<string> { "default", "seasonal" }),
            });
    }
}
