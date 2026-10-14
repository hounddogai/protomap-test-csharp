namespace Acme.Workforce.Assets.Warranty.Budget;

/// <summary>A rule a warranty budget must pass. Check returns its problems, or none.</summary>
public interface IWarrantyBudgetRule
{
    List<string> Check(WarrantyBudget item);
}

public sealed record WarrantyBudgetRequiredNameRule(string Message) : IWarrantyBudgetRule
{
    public List<string> Check(WarrantyBudget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBudgetRequiredReviewerRule(string Message) : IWarrantyBudgetRule
{
    public List<string> Check(WarrantyBudget item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBudgetRequiredCodeRule(string Message) : IWarrantyBudgetRule
{
    public List<string> Check(WarrantyBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty budget.</summary>
public sealed record WarrantyBudgetValidator(List<IWarrantyBudgetRule> Rules)
{
    public List<string> Validate(WarrantyBudget item)
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

    public void RequireValid(WarrantyBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyBudget> items)
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

/// <summary>Functions of warranty budget rules.</summary>
public static class WarrantyBudgetRules
{
    public static WarrantyBudgetValidator DefaultWarrantyBudgetValidator()
    {
        return new WarrantyBudgetValidator(
            new List<IWarrantyBudgetRule>
            {
                new WarrantyBudgetRequiredNameRule("warranty budget needs a value"),
                new WarrantyBudgetRequiredReviewerRule("warranty budget needs an entry"),
                new WarrantyBudgetRequiredCodeRule("warranty budget needs an entry"),
            });
    }
}
