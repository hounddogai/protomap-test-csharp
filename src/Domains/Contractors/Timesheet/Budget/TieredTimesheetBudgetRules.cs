namespace Acme.Workforce.Contractors.Timesheet.Budget;

/// <summary>A rule a tiered timesheet budget must pass. Check returns its problems, or none.</summary>
public interface ITieredTimesheetBudgetRule
{
    List<string> Check(TieredTimesheetBudget item);
}

public sealed record TieredTimesheetBudgetMinimumRangeRule(double Min, double Max) : ITieredTimesheetBudgetRule
{
    public List<string> Check(TieredTimesheetBudget item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTimesheetBudgetDepartmentLengthRule(int MaxLength) : ITieredTimesheetBudgetRule
{
    public List<string> Check(TieredTimesheetBudget item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTimesheetBudgetRequiredNameRule(string Message) : ITieredTimesheetBudgetRule
{
    public List<string> Check(TieredTimesheetBudget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredTimesheetBudgetCeilingRangeRule(double Min, double Max) : ITieredTimesheetBudgetRule
{
    public List<string> Check(TieredTimesheetBudget item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredTimesheetBudgetRequiredDepartmentRule(string Message) : ITieredTimesheetBudgetRule
{
    public List<string> Check(TieredTimesheetBudget item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered timesheet budget.</summary>
public sealed record TieredTimesheetBudgetValidator(List<ITieredTimesheetBudgetRule> Rules)
{
    public List<string> Validate(TieredTimesheetBudget item)
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

    public void RequireValid(TieredTimesheetBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredTimesheetBudget> items)
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

/// <summary>Functions of tiered timesheet budget rules.</summary>
public static class TieredTimesheetBudgetRules
{
    public static TieredTimesheetBudgetValidator DefaultTieredTimesheetBudgetValidator()
    {
        return new TieredTimesheetBudgetValidator(
            new List<ITieredTimesheetBudgetRule>
            {
                new TieredTimesheetBudgetMinimumRangeRule(9.1, 1692.2),
                new TieredTimesheetBudgetDepartmentLengthRule(86400),
                new TieredTimesheetBudgetRequiredNameRule("tiered timesheet budget needs a value"),
                new TieredTimesheetBudgetCeilingRangeRule(0.1, 3373.1),
                new TieredTimesheetBudgetRequiredDepartmentRule("tiered timesheet budget needs this field"),
            });
    }
}
