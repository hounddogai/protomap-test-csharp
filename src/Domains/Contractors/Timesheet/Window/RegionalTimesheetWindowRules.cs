namespace Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>A rule a regional timesheet window must pass. Check returns its problems, or none.</summary>
public interface IRegionalTimesheetWindowRule
{
    List<string> Check(RegionalTimesheetWindow item);
}

public sealed record RegionalTimesheetWindowRequiredTitleRule(string Message) : IRegionalTimesheetWindowRule
{
    public List<string> Check(RegionalTimesheetWindow item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetWindowRequiredSummaryRule(string Message) : IRegionalTimesheetWindowRule
{
    public List<string> Check(RegionalTimesheetWindow item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetWindowTenureMonthsRangeRule(int Min, int Max) : IRegionalTimesheetWindowRule
{
    public List<string> Check(RegionalTimesheetWindow item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional timesheet window.</summary>
public sealed record RegionalTimesheetWindowValidator(List<IRegionalTimesheetWindowRule> Rules)
{
    public List<string> Validate(RegionalTimesheetWindow item)
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

    public void RequireValid(RegionalTimesheetWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalTimesheetWindow> items)
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

/// <summary>Functions of regional timesheet window rules.</summary>
public static class RegionalTimesheetWindowRules
{
    public static RegionalTimesheetWindowValidator DefaultRegionalTimesheetWindowValidator()
    {
        return new RegionalTimesheetWindowValidator(
            new List<IRegionalTimesheetWindowRule>
            {
                new RegionalTimesheetWindowRequiredTitleRule("regional timesheet window needs a value"),
                new RegionalTimesheetWindowRequiredSummaryRule("regional timesheet window needs a value"),
                new RegionalTimesheetWindowTenureMonthsRangeRule(80, 1000),
            });
    }
}
