namespace Acme.Workforce.Contractors.Timesheet.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet preference must pass. Check returns its problems, or none.</summary>
public interface ITimesheetPreferenceRule
{
    List<string> Check(TimesheetPreference item);
}

public sealed record TimesheetPreferenceAllowedNameRule(List<string> Allowed) : ITimesheetPreferenceRule
{
    public List<string> Check(TimesheetPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetPreferenceNameLengthRule(int MaxLength) : ITimesheetPreferenceRule
{
    public List<string> Check(TimesheetPreference item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetPreferenceRequiredNameRule(string Message) : ITimesheetPreferenceRule
{
    public List<string> Check(TimesheetPreference item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet preference.</summary>
public sealed record TimesheetPreferenceValidator(List<ITimesheetPreferenceRule> Rules)
{
    public List<string> Validate(TimesheetPreference item)
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

    public void RequireValid(TimesheetPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetPreference> items)
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

/// <summary>Functions of timesheet preference rules.</summary>
public static class TimesheetPreferenceRules
{
    public static TimesheetPreferenceValidator DefaultTimesheetPreferenceValidator()
    {
        return new TimesheetPreferenceValidator(
            new List<ITimesheetPreferenceRule>
            {
                new TimesheetPreferenceAllowedNameRule(new List<string> { "fallback" }),
                new TimesheetPreferenceNameLengthRule(255),
                new TimesheetPreferenceRequiredNameRule("timesheet preference needs this field"),
            });
    }
}
