namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>A rule a payslip budget must pass. Check returns its problems, or none.</summary>
public interface IPayslipBudgetRule
{
    List<string> Check(PayslipBudget item);
}

public sealed record PayslipBudgetCodeLengthRule(int MaxLength) : IPayslipBudgetRule
{
    public List<string> Check(PayslipBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipBudgetRateRangeRule(double Min, double Max) : IPayslipBudgetRule
{
    public List<string> Check(PayslipBudget item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayslipBudgetRequiredTitleRule(string Message) : IPayslipBudgetRule
{
    public List<string> Check(PayslipBudget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipBudgetGraceDaysRangeRule(int Min, int Max) : IPayslipBudgetRule
{
    public List<string> Check(PayslipBudget item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a payslip budget.</summary>
public sealed record PayslipBudgetValidator(List<IPayslipBudgetRule> Rules)
{
    public List<string> Validate(PayslipBudget item)
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

    public void RequireValid(PayslipBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipBudget> items)
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

/// <summary>Functions of payslip budget rules.</summary>
public static class PayslipBudgetRules
{
    public static PayslipBudgetValidator DefaultPayslipBudgetValidator()
    {
        return new PayslipBudgetValidator(
            new List<IPayslipBudgetRule>
            {
                new PayslipBudgetCodeLengthRule(1000),
                new PayslipBudgetRateRangeRule(1.9, 3486.2),
                new PayslipBudgetRequiredTitleRule("payslip budget needs this field"),
                new PayslipBudgetGraceDaysRangeRule(1000, 1000),
            });
    }
}
