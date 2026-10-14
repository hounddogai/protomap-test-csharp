namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>A rule a timesheet plan must pass. Check returns its problems, or none.</summary>
public interface ITimesheetPlanRule
{
    List<string> Check(TimesheetPlan item);
}

public sealed record TimesheetPlanRequiredCodeRule(string Message) : ITimesheetPlanRule
{
    public List<string> Check(TimesheetPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetPlanQuantityRangeRule(int Min, int Max) : ITimesheetPlanRule
{
    public List<string> Check(TimesheetPlan item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TimesheetPlanAttemptsRangeRule(int Min, int Max) : ITimesheetPlanRule
{
    public List<string> Check(TimesheetPlan item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet plan.</summary>
public sealed record TimesheetPlanValidator(List<ITimesheetPlanRule> Rules)
{
    public List<string> Validate(TimesheetPlan item)
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

    public void RequireValid(TimesheetPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetPlan> items)
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

/// <summary>Functions of timesheet plan rules.</summary>
public static class TimesheetPlanRules
{
    public static TimesheetPlanValidator DefaultTimesheetPlanValidator()
    {
        return new TimesheetPlanValidator(
            new List<ITimesheetPlanRule>
            {
                new TimesheetPlanRequiredCodeRule("timesheet plan needs an entry"),
                new TimesheetPlanQuantityRangeRule(1000, 3600),
                new TimesheetPlanAttemptsRangeRule(32, 80),
            });
    }
}
