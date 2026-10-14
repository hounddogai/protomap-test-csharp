namespace Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>A rule a deduction budget must pass. Check returns its problems, or none.</summary>
public interface IDeductionBudgetRule
{
    List<string> Check(DeductionBudget item);
}

public sealed record DeductionBudgetMarginRangeRule(double Min, double Max) : IDeductionBudgetRule
{
    public List<string> Check(DeductionBudget item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeductionBudgetDepartmentLengthRule(int MaxLength) : IDeductionBudgetRule
{
    public List<string> Check(DeductionBudget item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionBudgetTenureMonthsRangeRule(int Min, int Max) : IDeductionBudgetRule
{
    public List<string> Check(DeductionBudget item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a deduction budget.</summary>
public sealed record DeductionBudgetValidator(List<IDeductionBudgetRule> Rules)
{
    public List<string> Validate(DeductionBudget item)
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

    public void RequireValid(DeductionBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionBudget> items)
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

/// <summary>Functions of deduction budget rules.</summary>
public static class DeductionBudgetRules
{
    public static DeductionBudgetValidator DefaultDeductionBudgetValidator()
    {
        return new DeductionBudgetValidator(
            new List<IDeductionBudgetRule>
            {
                new DeductionBudgetMarginRangeRule(6.7, 2132.3),
                new DeductionBudgetDepartmentLengthRule(1000),
                new DeductionBudgetTenureMonthsRangeRule(1000, 3600),
            });
    }
}
