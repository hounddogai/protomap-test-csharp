namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet assignment must pass. Check returns its problems, or none.</summary>
public interface ITimesheetAssignmentRule
{
    List<string> Check(TimesheetAssignment item);
}

public sealed record TimesheetAssignmentRequiredSalaryBandRule(string Message) : ITimesheetAssignmentRule
{
    public List<string> Check(TimesheetAssignment item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAssignmentExternalRefLengthRule(int MaxLength) : ITimesheetAssignmentRule
{
    public List<string> Check(TimesheetAssignment item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAssignmentFloorValueRangeRule(double Min, double Max) : ITimesheetAssignmentRule
{
    public List<string> Check(TimesheetAssignment item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAssignmentRequiredExternalRefRule(string Message) : ITimesheetAssignmentRule
{
    public List<string> Check(TimesheetAssignment item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAssignmentGraceDaysRangeRule(int Min, int Max) : ITimesheetAssignmentRule
{
    public List<string> Check(TimesheetAssignment item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAssignmentAllowedExternalRefRule(List<string> Allowed) : ITimesheetAssignmentRule
{
    public List<string> Check(TimesheetAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet assignment.</summary>
public sealed record TimesheetAssignmentValidator(List<ITimesheetAssignmentRule> Rules)
{
    public List<string> Validate(TimesheetAssignment item)
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

    public void RequireValid(TimesheetAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetAssignment> items)
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

/// <summary>Functions of timesheet assignment rules.</summary>
public static class TimesheetAssignmentRules
{
    public static TimesheetAssignmentValidator DefaultTimesheetAssignmentValidator()
    {
        return new TimesheetAssignmentValidator(
            new List<ITimesheetAssignmentRule>
            {
                new TimesheetAssignmentRequiredSalaryBandRule("timesheet assignment needs this field"),
                new TimesheetAssignmentExternalRefLengthRule(255),
                new TimesheetAssignmentFloorValueRangeRule(5.0, 4630.1),
                new TimesheetAssignmentRequiredExternalRefRule("timesheet assignment needs an entry"),
                new TimesheetAssignmentGraceDaysRangeRule(16, 80),
                new TimesheetAssignmentAllowedExternalRefRule(new List<string> { "default", "adaptive" }),
            });
    }
}
