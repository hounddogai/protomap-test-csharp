namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>A rule a timesheet group must pass. Check returns its problems, or none.</summary>
public interface ITimesheetGroupRule
{
    List<string> Check(TimesheetGroup item);
}

public sealed record TimesheetGroupRetriesRangeRule(int Min, int Max) : ITimesheetGroupRule
{
    public List<string> Check(TimesheetGroup item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TimesheetGroupRequiredTitleRule(string Message) : ITimesheetGroupRule
{
    public List<string> Check(TimesheetGroup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetGroupLocationLengthRule(int MaxLength) : ITimesheetGroupRule
{
    public List<string> Check(TimesheetGroup item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetGroupPinnedGuardRule(int Limit) : ITimesheetGroupRule
{
    public List<string> Check(TimesheetGroup item)
    {
        if (item.Pinned && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet group.</summary>
public sealed record TimesheetGroupValidator(List<ITimesheetGroupRule> Rules)
{
    public List<string> Validate(TimesheetGroup item)
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

    public void RequireValid(TimesheetGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetGroup> items)
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

/// <summary>Functions of timesheet group rules.</summary>
public static class TimesheetGroupRules
{
    public static TimesheetGroupValidator DefaultTimesheetGroupValidator()
    {
        return new TimesheetGroupValidator(
            new List<ITimesheetGroupRule>
            {
                new TimesheetGroupRetriesRangeRule(64, 255),
                new TimesheetGroupRequiredTitleRule("timesheet group needs a value"),
                new TimesheetGroupLocationLengthRule(80),
                new TimesheetGroupPinnedGuardRule(16),
            });
    }
}
