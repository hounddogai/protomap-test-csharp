namespace Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>A rule a capped timesheet plan must pass. Check returns its problems, or none.</summary>
public interface ICappedTimesheetPlanRule
{
    List<string> Check(CappedTimesheetPlan item);
}

public sealed record CappedTimesheetPlanMinimumRangeRule(double Min, double Max) : ICappedTimesheetPlanRule
{
    public List<string> Check(CappedTimesheetPlan item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedTimesheetPlanCodeLengthRule(int MaxLength) : ICappedTimesheetPlanRule
{
    public List<string> Check(CappedTimesheetPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedTimesheetPlanCapacityRangeRule(int Min, int Max) : ICappedTimesheetPlanRule
{
    public List<string> Check(CappedTimesheetPlan item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedTimesheetPlanRequiredCodeRule(string Message) : ICappedTimesheetPlanRule
{
    public List<string> Check(CappedTimesheetPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedTimesheetPlanWeightRangeRule(double Min, double Max) : ICappedTimesheetPlanRule
{
    public List<string> Check(CappedTimesheetPlan item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped timesheet plan.</summary>
public sealed record CappedTimesheetPlanValidator(List<ICappedTimesheetPlanRule> Rules)
{
    public List<string> Validate(CappedTimesheetPlan item)
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

    public void RequireValid(CappedTimesheetPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedTimesheetPlan> items)
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

/// <summary>Functions of capped timesheet plan rules.</summary>
public static class CappedTimesheetPlanRules
{
    public static CappedTimesheetPlanValidator DefaultCappedTimesheetPlanValidator()
    {
        return new CappedTimesheetPlanValidator(
            new List<ICappedTimesheetPlanRule>
            {
                new CappedTimesheetPlanMinimumRangeRule(8.6, 3408.2),
                new CappedTimesheetPlanCodeLengthRule(120),
                new CappedTimesheetPlanCapacityRangeRule(64, 255),
                new CappedTimesheetPlanRequiredCodeRule("capped timesheet plan needs an entry"),
                new CappedTimesheetPlanWeightRangeRule(6.5, 2820.6),
            });
    }
}
