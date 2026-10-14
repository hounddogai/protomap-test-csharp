namespace Acme.Workforce.Contractors.Timesheet.Group;

/// <summary>A rule a scheduled timesheet group must pass. Check returns its problems, or none.</summary>
public interface IScheduledTimesheetGroupRule
{
    List<string> Check(ScheduledTimesheetGroup item);
}

public sealed record ScheduledTimesheetGroupEndsAtRangeRule(int Min, int Max) : IScheduledTimesheetGroupRule
{
    public List<string> Check(ScheduledTimesheetGroup item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTimesheetGroupThresholdRangeRule(double Min, double Max) : IScheduledTimesheetGroupRule
{
    public List<string> Check(ScheduledTimesheetGroup item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledTimesheetGroupRequiredLabelRule(string Message) : IScheduledTimesheetGroupRule
{
    public List<string> Check(ScheduledTimesheetGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled timesheet group.</summary>
public sealed record ScheduledTimesheetGroupValidator(List<IScheduledTimesheetGroupRule> Rules)
{
    public List<string> Validate(ScheduledTimesheetGroup item)
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

    public void RequireValid(ScheduledTimesheetGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledTimesheetGroup> items)
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

/// <summary>Functions of scheduled timesheet group rules.</summary>
public static class ScheduledTimesheetGroupRules
{
    public static ScheduledTimesheetGroupValidator DefaultScheduledTimesheetGroupValidator()
    {
        return new ScheduledTimesheetGroupValidator(
            new List<IScheduledTimesheetGroupRule>
            {
                new ScheduledTimesheetGroupEndsAtRangeRule(1000, 86400),
                new ScheduledTimesheetGroupThresholdRangeRule(8.5, 3648.0),
                new ScheduledTimesheetGroupRequiredLabelRule("scheduled timesheet group needs a value"),
            });
    }
}
