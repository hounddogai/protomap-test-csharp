namespace Acme.Workforce.Contractors.Timesheet.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet baseline must pass. Check returns its problems, or none.</summary>
public interface ITimesheetBaselineRule
{
    List<string> Check(TimesheetBaseline item);
}

public sealed record TimesheetBaselineRequiredTitleRule(string Message) : ITimesheetBaselineRule
{
    public List<string> Check(TimesheetBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBaselineCodeLengthRule(int MaxLength) : ITimesheetBaselineRule
{
    public List<string> Check(TimesheetBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBaselineNameLengthRule(int MaxLength) : ITimesheetBaselineRule
{
    public List<string> Check(TimesheetBaseline item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBaselineAllowedTitleRule(List<string> Allowed) : ITimesheetBaselineRule
{
    public List<string> Check(TimesheetBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBaselineTitleLengthRule(int MaxLength) : ITimesheetBaselineRule
{
    public List<string> Check(TimesheetBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBaselineCodePrefixRule(string Prefix) : ITimesheetBaselineRule
{
    public List<string> Check(TimesheetBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet baseline.</summary>
public sealed record TimesheetBaselineValidator(List<ITimesheetBaselineRule> Rules)
{
    public List<string> Validate(TimesheetBaseline item)
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

    public void RequireValid(TimesheetBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetBaseline> items)
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

/// <summary>Functions of timesheet baseline rules.</summary>
public static class TimesheetBaselineRules
{
    public static TimesheetBaselineValidator DefaultTimesheetBaselineValidator()
    {
        return new TimesheetBaselineValidator(
            new List<ITimesheetBaselineRule>
            {
                new TimesheetBaselineRequiredTitleRule("timesheet baseline needs this field"),
                new TimesheetBaselineCodeLengthRule(255),
                new TimesheetBaselineNameLengthRule(86400),
                new TimesheetBaselineAllowedTitleRule(new List<string>()),
                new TimesheetBaselineTitleLengthRule(3600),
                new TimesheetBaselineCodePrefixRule("X"),
            });
    }
}
