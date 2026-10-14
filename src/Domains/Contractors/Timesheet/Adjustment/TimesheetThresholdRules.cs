namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>A rule a timesheet threshold must pass. Check returns its problems, or none.</summary>
public interface ITimesheetThresholdRule
{
    List<string> Check(TimesheetThreshold item);
}

public sealed record TimesheetThresholdRequiredCodeRule(string Message) : ITimesheetThresholdRule
{
    public List<string> Check(TimesheetThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetThresholdMaximumRangeRule(double Min, double Max) : ITimesheetThresholdRule
{
    public List<string> Check(TimesheetThreshold item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TimesheetThresholdCodeLengthRule(int MaxLength) : ITimesheetThresholdRule
{
    public List<string> Check(TimesheetThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetThresholdPriorityRangeRule(int Min, int Max) : ITimesheetThresholdRule
{
    public List<string> Check(TimesheetThreshold item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetThresholdJobCodeLengthRule(int MaxLength) : ITimesheetThresholdRule
{
    public List<string> Check(TimesheetThreshold item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet threshold.</summary>
public sealed record TimesheetThresholdValidator(List<ITimesheetThresholdRule> Rules)
{
    public List<string> Validate(TimesheetThreshold item)
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

    public void RequireValid(TimesheetThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetThreshold> items)
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

/// <summary>Functions of timesheet threshold rules.</summary>
public static class TimesheetThresholdRules
{
    public static TimesheetThresholdValidator DefaultTimesheetThresholdValidator()
    {
        return new TimesheetThresholdValidator(
            new List<ITimesheetThresholdRule>
            {
                new TimesheetThresholdRequiredCodeRule("timesheet threshold needs an entry"),
                new TimesheetThresholdMaximumRangeRule(9.3, 2251.3),
                new TimesheetThresholdCodeLengthRule(80),
                new TimesheetThresholdPriorityRangeRule(16, 86400),
                new TimesheetThresholdJobCodeLengthRule(120),
            });
    }
}
