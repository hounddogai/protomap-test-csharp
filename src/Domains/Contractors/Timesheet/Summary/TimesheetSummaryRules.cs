namespace Acme.Workforce.Contractors.Timesheet.Summary;

/// <summary>A rule a timesheet summary must pass. Check returns its problems, or none.</summary>
public interface ITimesheetSummaryRule
{
    List<string> Check(TimesheetSummary item);
}

public sealed record TimesheetSummaryRequiredPhoneRule(string Message) : ITimesheetSummaryRule
{
    public List<string> Check(TimesheetSummary item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetSummaryRequiredNameRule(string Message) : ITimesheetSummaryRule
{
    public List<string> Check(TimesheetSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetSummaryRequiredCodeRule(string Message) : ITimesheetSummaryRule
{
    public List<string> Check(TimesheetSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetSummaryRatioRangeRule(double Min, double Max) : ITimesheetSummaryRule
{
    public List<string> Check(TimesheetSummary item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetSummaryNameLengthRule(int MaxLength) : ITimesheetSummaryRule
{
    public List<string> Check(TimesheetSummary item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet summary.</summary>
public sealed record TimesheetSummaryValidator(List<ITimesheetSummaryRule> Rules)
{
    public List<string> Validate(TimesheetSummary item)
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

    public void RequireValid(TimesheetSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetSummary> items)
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

/// <summary>Functions of timesheet summary rules.</summary>
public static class TimesheetSummaryRules
{
    public static TimesheetSummaryValidator DefaultTimesheetSummaryValidator()
    {
        return new TimesheetSummaryValidator(
            new List<ITimesheetSummaryRule>
            {
                new TimesheetSummaryRequiredPhoneRule("timesheet summary needs a value"),
                new TimesheetSummaryRequiredNameRule("timesheet summary needs this field"),
                new TimesheetSummaryRequiredCodeRule("timesheet summary needs this field"),
                new TimesheetSummaryRatioRangeRule(3.7, 974.2),
                new TimesheetSummaryNameLengthRule(120),
            });
    }
}
