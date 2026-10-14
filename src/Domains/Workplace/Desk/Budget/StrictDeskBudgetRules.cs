namespace Acme.Workforce.Workplace.Desk.Budget;

/// <summary>A rule a strict desk budget must pass. Check returns its problems, or none.</summary>
public interface IStrictDeskBudgetRule
{
    List<string> Check(StrictDeskBudget item);
}

public sealed record StrictDeskBudgetRequiredCodeRule(string Message) : IStrictDeskBudgetRule
{
    public List<string> Check(StrictDeskBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictDeskBudgetRequiredUnitRule(string Message) : IStrictDeskBudgetRule
{
    public List<string> Check(StrictDeskBudget item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictDeskBudgetRequiredLabelRule(string Message) : IStrictDeskBudgetRule
{
    public List<string> Check(StrictDeskBudget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictDeskBudgetUnitLengthRule(int MaxLength) : IStrictDeskBudgetRule
{
    public List<string> Check(StrictDeskBudget item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict desk budget.</summary>
public sealed record StrictDeskBudgetValidator(List<IStrictDeskBudgetRule> Rules)
{
    public List<string> Validate(StrictDeskBudget item)
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

    public void RequireValid(StrictDeskBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictDeskBudget> items)
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

/// <summary>Functions of strict desk budget rules.</summary>
public static class StrictDeskBudgetRules
{
    public static StrictDeskBudgetValidator DefaultStrictDeskBudgetValidator()
    {
        return new StrictDeskBudgetValidator(
            new List<IStrictDeskBudgetRule>
            {
                new StrictDeskBudgetRequiredCodeRule("strict desk budget needs this field"),
                new StrictDeskBudgetRequiredUnitRule("strict desk budget needs this field"),
                new StrictDeskBudgetRequiredLabelRule("strict desk budget needs this field"),
                new StrictDeskBudgetUnitLengthRule(16),
            });
    }
}
