namespace Acme.Workforce.Contractors.Timesheet.Score;

/// <summary>A rule a scheduled timesheet score must pass. Check returns its problems, or none.</summary>
public interface IScheduledTimesheetScoreRule
{
    List<string> Check(ScheduledTimesheetScore item);
}

public sealed record ScheduledTimesheetScoreRequiredSegmentRule(string Message) : IScheduledTimesheetScoreRule
{
    public List<string> Check(ScheduledTimesheetScore item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTimesheetScoreTitleLengthRule(int MaxLength) : IScheduledTimesheetScoreRule
{
    public List<string> Check(ScheduledTimesheetScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTimesheetScoreApprovedGuardRule(int Limit) : IScheduledTimesheetScoreRule
{
    public List<string> Check(ScheduledTimesheetScore item)
    {
        if (item.Approved && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTimesheetScoreRequiredCodeRule(string Message) : IScheduledTimesheetScoreRule
{
    public List<string> Check(ScheduledTimesheetScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled timesheet score.</summary>
public sealed record ScheduledTimesheetScoreValidator(List<IScheduledTimesheetScoreRule> Rules)
{
    public List<string> Validate(ScheduledTimesheetScore item)
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

    public void RequireValid(ScheduledTimesheetScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledTimesheetScore> items)
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

/// <summary>Functions of scheduled timesheet score rules.</summary>
public static class ScheduledTimesheetScoreRules
{
    public static ScheduledTimesheetScoreValidator DefaultScheduledTimesheetScoreValidator()
    {
        return new ScheduledTimesheetScoreValidator(
            new List<IScheduledTimesheetScoreRule>
            {
                new ScheduledTimesheetScoreRequiredSegmentRule("scheduled timesheet score needs this field"),
                new ScheduledTimesheetScoreTitleLengthRule(1000),
                new ScheduledTimesheetScoreApprovedGuardRule(86400),
                new ScheduledTimesheetScoreRequiredCodeRule("scheduled timesheet score needs this field"),
            });
    }
}
