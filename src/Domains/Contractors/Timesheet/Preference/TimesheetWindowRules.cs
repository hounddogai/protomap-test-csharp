namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet window must pass. Check returns its problems, or none.</summary>
public interface ITimesheetWindowRule
{
    List<string> Check(TimesheetWindow item);
}

public sealed record TimesheetWindowRequiredTimezoneRule(string Message) : ITimesheetWindowRule
{
    public List<string> Check(TimesheetWindow item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetWindowVersionRangeRule(int Min, int Max) : ITimesheetWindowRule
{
    public List<string> Check(TimesheetWindow item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TimesheetWindowTimezoneLengthRule(int MaxLength) : ITimesheetWindowRule
{
    public List<string> Check(TimesheetWindow item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetWindowAllowedLocationRule(List<string> Allowed) : ITimesheetWindowRule
{
    public List<string> Check(TimesheetWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetWindowScoreRangeRule(double Min, double Max) : ITimesheetWindowRule
{
    public List<string> Check(TimesheetWindow item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a timesheet window.</summary>
public sealed record TimesheetWindowValidator(List<ITimesheetWindowRule> Rules)
{
    public List<string> Validate(TimesheetWindow item)
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

    public void RequireValid(TimesheetWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetWindow> items)
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

/// <summary>Functions of timesheet window rules.</summary>
public static class TimesheetWindowRules
{
    public static TimesheetWindowValidator DefaultTimesheetWindowValidator()
    {
        return new TimesheetWindowValidator(
            new List<ITimesheetWindowRule>
            {
                new TimesheetWindowRequiredTimezoneRule("timesheet window needs a value"),
                new TimesheetWindowVersionRangeRule(32, 255),
                new TimesheetWindowTimezoneLengthRule(3600),
                new TimesheetWindowAllowedLocationRule(new List<string> { "legacy", "manual", "regional" }),
                new TimesheetWindowScoreRangeRule(3.7, 2640.7),
            });
    }
}
