namespace Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>A rule a weighted payslip budget must pass. Check returns its problems, or none.</summary>
public interface IWeightedPayslipBudgetRule
{
    List<string> Check(WeightedPayslipBudget item);
}

public sealed record WeightedPayslipBudgetTitleLengthRule(int MaxLength) : IWeightedPayslipBudgetRule
{
    public List<string> Check(WeightedPayslipBudget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayslipBudgetRetriesRangeRule(int Min, int Max) : IWeightedPayslipBudgetRule
{
    public List<string> Check(WeightedPayslipBudget item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayslipBudgetRequiredTitleRule(string Message) : IWeightedPayslipBudgetRule
{
    public List<string> Check(WeightedPayslipBudget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted payslip budget.</summary>
public sealed record WeightedPayslipBudgetValidator(List<IWeightedPayslipBudgetRule> Rules)
{
    public List<string> Validate(WeightedPayslipBudget item)
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

    public void RequireValid(WeightedPayslipBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedPayslipBudget> items)
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

/// <summary>Functions of weighted payslip budget rules.</summary>
public static class WeightedPayslipBudgetRules
{
    public static WeightedPayslipBudgetValidator DefaultWeightedPayslipBudgetValidator()
    {
        return new WeightedPayslipBudgetValidator(
            new List<IWeightedPayslipBudgetRule>
            {
                new WeightedPayslipBudgetTitleLengthRule(64),
                new WeightedPayslipBudgetRetriesRangeRule(16, 86400),
                new WeightedPayslipBudgetRequiredTitleRule("weighted payslip budget needs this field"),
            });
    }
}
