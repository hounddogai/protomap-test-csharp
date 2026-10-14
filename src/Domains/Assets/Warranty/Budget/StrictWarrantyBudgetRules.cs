namespace Acme.Workforce.Assets.Warranty.Budget;

/// <summary>A rule a strict warranty budget must pass. Check returns its problems, or none.</summary>
public interface IStrictWarrantyBudgetRule
{
    List<string> Check(StrictWarrantyBudget item);
}

public sealed record StrictWarrantyBudgetSourceLengthRule(int MaxLength) : IStrictWarrantyBudgetRule
{
    public List<string> Check(StrictWarrantyBudget item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictWarrantyBudgetCodeLengthRule(int MaxLength) : IStrictWarrantyBudgetRule
{
    public List<string> Check(StrictWarrantyBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictWarrantyBudgetRequiredTitleRule(string Message) : IStrictWarrantyBudgetRule
{
    public List<string> Check(StrictWarrantyBudget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict warranty budget.</summary>
public sealed record StrictWarrantyBudgetValidator(List<IStrictWarrantyBudgetRule> Rules)
{
    public List<string> Validate(StrictWarrantyBudget item)
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

    public void RequireValid(StrictWarrantyBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictWarrantyBudget> items)
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

/// <summary>Functions of strict warranty budget rules.</summary>
public static class StrictWarrantyBudgetRules
{
    public static StrictWarrantyBudgetValidator DefaultStrictWarrantyBudgetValidator()
    {
        return new StrictWarrantyBudgetValidator(
            new List<IStrictWarrantyBudgetRule>
            {
                new StrictWarrantyBudgetSourceLengthRule(32),
                new StrictWarrantyBudgetCodeLengthRule(32),
                new StrictWarrantyBudgetRequiredTitleRule("strict warranty budget needs an entry"),
            });
    }
}
