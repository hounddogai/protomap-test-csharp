namespace Acme.Workforce.Contractors.Timesheet.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet milestone must pass. Check returns its problems, or none.</summary>
public interface ITimesheetMilestoneRule
{
    List<string> Check(TimesheetMilestone item);
}

public sealed record TimesheetMilestoneRequiredCodeRule(string Message) : ITimesheetMilestoneRule
{
    public List<string> Check(TimesheetMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetMilestoneReviewerLengthRule(int MaxLength) : ITimesheetMilestoneRule
{
    public List<string> Check(TimesheetMilestone item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetMilestoneAllowedCodeRule(List<string> Allowed) : ITimesheetMilestoneRule
{
    public List<string> Check(TimesheetMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet milestone.</summary>
public sealed record TimesheetMilestoneValidator(List<ITimesheetMilestoneRule> Rules)
{
    public List<string> Validate(TimesheetMilestone item)
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

    public void RequireValid(TimesheetMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetMilestone> items)
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

/// <summary>Functions of timesheet milestone rules.</summary>
public static class TimesheetMilestoneRules
{
    public static TimesheetMilestoneValidator DefaultTimesheetMilestoneValidator()
    {
        return new TimesheetMilestoneValidator(
            new List<ITimesheetMilestoneRule>
            {
                new TimesheetMilestoneRequiredCodeRule("timesheet milestone needs a value"),
                new TimesheetMilestoneReviewerLengthRule(120),
                new TimesheetMilestoneAllowedCodeRule(new List<string> { "global", "seasonal", "global" }),
            });
    }
}
