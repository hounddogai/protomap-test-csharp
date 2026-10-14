namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet segment must pass. Check returns its problems, or none.</summary>
public interface ITimesheetSegmentRule
{
    List<string> Check(TimesheetSegment item);
}

public sealed record TimesheetSegmentRequiredCodeRule(string Message) : ITimesheetSegmentRule
{
    public List<string> Check(TimesheetSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetSegmentAllowedNameRule(List<string> Allowed) : ITimesheetSegmentRule
{
    public List<string> Check(TimesheetSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetSegmentNameLengthRule(int MaxLength) : ITimesheetSegmentRule
{
    public List<string> Check(TimesheetSegment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet segment.</summary>
public sealed record TimesheetSegmentValidator(List<ITimesheetSegmentRule> Rules)
{
    public List<string> Validate(TimesheetSegment item)
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

    public void RequireValid(TimesheetSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetSegment> items)
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

/// <summary>Functions of timesheet segment rules.</summary>
public static class TimesheetSegmentRules
{
    public static TimesheetSegmentValidator DefaultTimesheetSegmentValidator()
    {
        return new TimesheetSegmentValidator(
            new List<ITimesheetSegmentRule>
            {
                new TimesheetSegmentRequiredCodeRule("timesheet segment needs this field"),
                new TimesheetSegmentAllowedNameRule(new List<string> { "manual", "seasonal", "adaptive" }),
                new TimesheetSegmentNameLengthRule(86400),
            });
    }
}
