namespace Acme.Workforce.Talent.Pool.Budget;

/// <summary>A rule a pool budget must pass. Check returns its problems, or none.</summary>
public interface IPoolBudgetRule
{
    List<string> Check(PoolBudget item);
}

public sealed record PoolBudgetRatioRangeRule(double Min, double Max) : IPoolBudgetRule
{
    public List<string> Check(PoolBudget item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolBudgetRequiredCostCenterRule(string Message) : IPoolBudgetRule
{
    public List<string> Check(PoolBudget item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolBudgetOwnerIdLengthRule(int MaxLength) : IPoolBudgetRule
{
    public List<string> Check(PoolBudget item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolBudgetRequiredOwnerIdRule(string Message) : IPoolBudgetRule
{
    public List<string> Check(PoolBudget item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolBudgetNameLengthRule(int MaxLength) : IPoolBudgetRule
{
    public List<string> Check(PoolBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool budget.</summary>
public sealed record PoolBudgetValidator(List<IPoolBudgetRule> Rules)
{
    public List<string> Validate(PoolBudget item)
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

    public void RequireValid(PoolBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolBudget> items)
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

/// <summary>Functions of pool budget rules.</summary>
public static class PoolBudgetRules
{
    public static PoolBudgetValidator DefaultPoolBudgetValidator()
    {
        return new PoolBudgetValidator(
            new List<IPoolBudgetRule>
            {
                new PoolBudgetRatioRangeRule(6.7, 3376.6),
                new PoolBudgetRequiredCostCenterRule("pool budget needs this field"),
                new PoolBudgetOwnerIdLengthRule(80),
                new PoolBudgetRequiredOwnerIdRule("pool budget needs a value"),
                new PoolBudgetNameLengthRule(16),
            });
    }
}
