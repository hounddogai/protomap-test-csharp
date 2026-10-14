namespace Acme.Workforce.Absence.Policy.Budget;

/// <summary>A rule a fallback policy budget must pass. Check returns its problems, or none.</summary>
public interface IFallbackPolicyBudgetRule
{
    List<string> Check(FallbackPolicyBudget item);
}

public sealed record FallbackPolicyBudgetContactEmailLengthRule(int MaxLength) : IFallbackPolicyBudgetRule
{
    public List<string> Check(FallbackPolicyBudget item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyBudgetMinimumRangeRule(double Min, double Max) : IFallbackPolicyBudgetRule
{
    public List<string> Check(FallbackPolicyBudget item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackPolicyBudgetEmployeeIdLengthRule(int MaxLength) : IFallbackPolicyBudgetRule
{
    public List<string> Check(FallbackPolicyBudget item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback policy budget.</summary>
public sealed record FallbackPolicyBudgetValidator(List<IFallbackPolicyBudgetRule> Rules)
{
    public List<string> Validate(FallbackPolicyBudget item)
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

    public void RequireValid(FallbackPolicyBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPolicyBudget> items)
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

/// <summary>Functions of fallback policy budget rules.</summary>
public static class FallbackPolicyBudgetRules
{
    public static FallbackPolicyBudgetValidator DefaultFallbackPolicyBudgetValidator()
    {
        return new FallbackPolicyBudgetValidator(
            new List<IFallbackPolicyBudgetRule>
            {
                new FallbackPolicyBudgetContactEmailLengthRule(80),
                new FallbackPolicyBudgetMinimumRangeRule(5.5, 4535.7),
                new FallbackPolicyBudgetEmployeeIdLengthRule(1000),
            });
    }
}
