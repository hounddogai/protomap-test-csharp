namespace Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>A rule a timesheet allocation must pass. Check returns its problems, or none.</summary>
public interface ITimesheetAllocationRule
{
    List<string> Check(TimesheetAllocation item);
}

public sealed record TimesheetAllocationCodeLengthRule(int MaxLength) : ITimesheetAllocationRule
{
    public List<string> Check(TimesheetAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAllocationSegmentLengthRule(int MaxLength) : ITimesheetAllocationRule
{
    public List<string> Check(TimesheetAllocation item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAllocationWeightRangeRule(double Min, double Max) : ITimesheetAllocationRule
{
    public List<string> Check(TimesheetAllocation item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet allocation.</summary>
public sealed record TimesheetAllocationValidator(List<ITimesheetAllocationRule> Rules)
{
    public List<string> Validate(TimesheetAllocation item)
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

    public void RequireValid(TimesheetAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetAllocation> items)
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

/// <summary>Functions of timesheet allocation rules.</summary>
public static class TimesheetAllocationRules
{
    public static TimesheetAllocationValidator DefaultTimesheetAllocationValidator()
    {
        return new TimesheetAllocationValidator(
            new List<ITimesheetAllocationRule>
            {
                new TimesheetAllocationCodeLengthRule(80),
                new TimesheetAllocationSegmentLengthRule(16),
                new TimesheetAllocationWeightRangeRule(6.3, 314.7),
            });
    }
}
