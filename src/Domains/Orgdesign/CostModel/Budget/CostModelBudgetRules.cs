namespace Acme.Workforce.Orgdesign.CostModel.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model budget must pass. Check returns its problems, or none.</summary>
public interface ICostModelBudgetRule
{
    List<string> Check(CostModelBudget item);
}

public sealed record CostModelBudgetRequiredTitleRule(string Message) : ICostModelBudgetRule
{
    public List<string> Check(CostModelBudget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelBudgetRequiredOwnerIdRule(string Message) : ICostModelBudgetRule
{
    public List<string> Check(CostModelBudget item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelBudgetRequiredCodeRule(string Message) : ICostModelBudgetRule
{
    public List<string> Check(CostModelBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelBudgetAllowedOwnerIdRule(List<string> Allowed) : ICostModelBudgetRule
{
    public List<string> Check(CostModelBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record CostModelBudgetOwnerIdLengthRule(int MaxLength) : ICostModelBudgetRule
{
    public List<string> Check(CostModelBudget item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelBudgetTitleLengthRule(int MaxLength) : ICostModelBudgetRule
{
    public List<string> Check(CostModelBudget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model budget.</summary>
public sealed record CostModelBudgetValidator(List<ICostModelBudgetRule> Rules)
{
    public List<string> Validate(CostModelBudget item)
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

    public void RequireValid(CostModelBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelBudget> items)
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

/// <summary>Functions of cost model budget rules.</summary>
public static class CostModelBudgetRules
{
    public static CostModelBudgetValidator DefaultCostModelBudgetValidator()
    {
        return new CostModelBudgetValidator(
            new List<ICostModelBudgetRule>
            {
                new CostModelBudgetRequiredTitleRule("cost model budget needs an entry"),
                new CostModelBudgetRequiredOwnerIdRule("cost model budget needs this field"),
                new CostModelBudgetRequiredCodeRule("cost model budget needs an entry"),
                new CostModelBudgetAllowedOwnerIdRule(new List<string>()),
                new CostModelBudgetOwnerIdLengthRule(1000),
                new CostModelBudgetTitleLengthRule(255),
            });
    }
}
