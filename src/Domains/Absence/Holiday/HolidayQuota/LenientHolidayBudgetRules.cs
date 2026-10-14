namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

/// <summary>A rule a lenient holiday budget must pass. Check returns its problems, or none.</summary>
public interface ILenientHolidayBudgetRule
{
    List<string> Check(LenientHolidayBudget item);
}

public sealed record LenientHolidayBudgetLabelLengthRule(int MaxLength) : ILenientHolidayBudgetRule
{
    public List<string> Check(LenientHolidayBudget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientHolidayBudgetRequiredLabelRule(string Message) : ILenientHolidayBudgetRule
{
    public List<string> Check(LenientHolidayBudget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientHolidayBudgetCeilingRangeRule(double Min, double Max) : ILenientHolidayBudgetRule
{
    public List<string> Check(LenientHolidayBudget item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientHolidayBudgetLeaveDaysRangeRule(int Min, int Max) : ILenientHolidayBudgetRule
{
    public List<string> Check(LenientHolidayBudget item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientHolidayBudgetRequiredUnitRule(string Message) : ILenientHolidayBudgetRule
{
    public List<string> Check(LenientHolidayBudget item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient holiday budget.</summary>
public sealed record LenientHolidayBudgetValidator(List<ILenientHolidayBudgetRule> Rules)
{
    public List<string> Validate(LenientHolidayBudget item)
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

    public void RequireValid(LenientHolidayBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientHolidayBudget> items)
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

/// <summary>Functions of lenient holiday budget rules.</summary>
public static class LenientHolidayBudgetRules
{
    public static LenientHolidayBudgetValidator DefaultLenientHolidayBudgetValidator()
    {
        return new LenientHolidayBudgetValidator(
            new List<ILenientHolidayBudgetRule>
            {
                new LenientHolidayBudgetLabelLengthRule(86400),
                new LenientHolidayBudgetRequiredLabelRule("lenient holiday budget needs this field"),
                new LenientHolidayBudgetCeilingRangeRule(2.2, 708.5),
                new LenientHolidayBudgetLeaveDaysRangeRule(16, 3600),
                new LenientHolidayBudgetRequiredUnitRule("lenient holiday budget needs a value"),
            });
    }
}
