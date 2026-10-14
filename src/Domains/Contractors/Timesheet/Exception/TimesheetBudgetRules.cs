namespace Acme.Workforce.Contractors.Timesheet.Exception;

/// <summary>A rule a timesheet budget must pass. Check returns its problems, or none.</summary>
public interface ITimesheetBudgetRule
{
    List<string> Check(TimesheetBudget item);
}

public sealed record TimesheetBudgetNameLengthRule(int MaxLength) : ITimesheetBudgetRule
{
    public List<string> Check(TimesheetBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBudgetRequiredSegmentRule(string Message) : ITimesheetBudgetRule
{
    public List<string> Check(TimesheetBudget item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBudgetSegmentLengthRule(int MaxLength) : ITimesheetBudgetRule
{
    public List<string> Check(TimesheetBudget item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBudgetCountryLengthRule(int MaxLength) : ITimesheetBudgetRule
{
    public List<string> Check(TimesheetBudget item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBudgetCodeLengthRule(int MaxLength) : ITimesheetBudgetRule
{
    public List<string> Check(TimesheetBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet budget.</summary>
public sealed record TimesheetBudgetValidator(List<ITimesheetBudgetRule> Rules)
{
    public List<string> Validate(TimesheetBudget item)
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

    public void RequireValid(TimesheetBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetBudget> items)
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

/// <summary>Functions of timesheet budget rules.</summary>
public static class TimesheetBudgetRules
{
    public static TimesheetBudgetValidator DefaultTimesheetBudgetValidator()
    {
        return new TimesheetBudgetValidator(
            new List<ITimesheetBudgetRule>
            {
                new TimesheetBudgetNameLengthRule(120),
                new TimesheetBudgetRequiredSegmentRule("timesheet budget needs an entry"),
                new TimesheetBudgetSegmentLengthRule(64),
                new TimesheetBudgetCountryLengthRule(32),
                new TimesheetBudgetCodeLengthRule(120),
            });
    }
}
