namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>A rule a batched deduction budget must pass. Check returns its problems, or none.</summary>
public interface IBatchedDeductionBudgetRule
{
    List<string> Check(BatchedDeductionBudget item);
}

public sealed record BatchedDeductionBudgetExpiresAtRangeRule(int Min, int Max) : IBatchedDeductionBudgetRule
{
    public List<string> Check(BatchedDeductionBudget item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedDeductionBudgetRequiredCodeRule(string Message) : IBatchedDeductionBudgetRule
{
    public List<string> Check(BatchedDeductionBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedDeductionBudgetEmployeeIdLengthRule(int MaxLength) : IBatchedDeductionBudgetRule
{
    public List<string> Check(BatchedDeductionBudget item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedDeductionBudgetCodeLengthRule(int MaxLength) : IBatchedDeductionBudgetRule
{
    public List<string> Check(BatchedDeductionBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched deduction budget.</summary>
public sealed record BatchedDeductionBudgetValidator(List<IBatchedDeductionBudgetRule> Rules)
{
    public List<string> Validate(BatchedDeductionBudget item)
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

    public void RequireValid(BatchedDeductionBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedDeductionBudget> items)
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

/// <summary>Functions of batched deduction budget rules.</summary>
public static class BatchedDeductionBudgetRules
{
    public static BatchedDeductionBudgetValidator DefaultBatchedDeductionBudgetValidator()
    {
        return new BatchedDeductionBudgetValidator(
            new List<IBatchedDeductionBudgetRule>
            {
                new BatchedDeductionBudgetExpiresAtRangeRule(1000, 3600),
                new BatchedDeductionBudgetRequiredCodeRule("batched deduction budget needs a value"),
                new BatchedDeductionBudgetEmployeeIdLengthRule(86400),
                new BatchedDeductionBudgetCodeLengthRule(16),
            });
    }
}
