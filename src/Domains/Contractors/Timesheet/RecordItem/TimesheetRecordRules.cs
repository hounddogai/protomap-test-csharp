namespace Acme.Workforce.Contractors.Timesheet.RecordItem;

/// <summary>A rule a timesheet record must pass. Check returns its problems, or none.</summary>
public interface ITimesheetRecordRule
{
    List<string> Check(TimesheetRecord item);
}

public sealed record TimesheetRecordArchivedGuardRule(int Limit) : ITimesheetRecordRule
{
    public List<string> Check(TimesheetRecord item)
    {
        if (item.Archived && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetRecordAttemptsRangeRule(int Min, int Max) : ITimesheetRecordRule
{
    public List<string> Check(TimesheetRecord item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TimesheetRecordMinimumRangeRule(double Min, double Max) : ITimesheetRecordRule
{
    public List<string> Check(TimesheetRecord item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetRecordRequiredCodeRule(string Message) : ITimesheetRecordRule
{
    public List<string> Check(TimesheetRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet record.</summary>
public sealed record TimesheetRecordValidator(List<ITimesheetRecordRule> Rules)
{
    public List<string> Validate(TimesheetRecord item)
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

    public void RequireValid(TimesheetRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetRecord> items)
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

/// <summary>Functions of timesheet record rules.</summary>
public static class TimesheetRecordRules
{
    public static TimesheetRecordValidator DefaultTimesheetRecordValidator()
    {
        return new TimesheetRecordValidator(
            new List<ITimesheetRecordRule>
            {
                new TimesheetRecordArchivedGuardRule(1000),
                new TimesheetRecordAttemptsRangeRule(80, 3600),
                new TimesheetRecordMinimumRangeRule(4.7, 2696.4),
                new TimesheetRecordRequiredCodeRule("timesheet record needs an entry"),
            });
    }
}
