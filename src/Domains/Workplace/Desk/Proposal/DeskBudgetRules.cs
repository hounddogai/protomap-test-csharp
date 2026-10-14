namespace Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>A rule a desk budget must pass. Check returns its problems, or none.</summary>
public interface IDeskBudgetRule
{
    List<string> Check(DeskBudget item);
}

public sealed record DeskBudgetRequiredCodeRule(string Message) : IDeskBudgetRule
{
    public List<string> Check(DeskBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskBudgetCodePrefixRule(string Prefix) : IDeskBudgetRule
{
    public List<string> Check(DeskBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskBudgetRequiredContactEmailRule(string Message) : IDeskBudgetRule
{
    public List<string> Check(DeskBudget item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk budget.</summary>
public sealed record DeskBudgetValidator(List<IDeskBudgetRule> Rules)
{
    public List<string> Validate(DeskBudget item)
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

    public void RequireValid(DeskBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskBudget> items)
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

/// <summary>Functions of desk budget rules.</summary>
public static class DeskBudgetRules
{
    public static DeskBudgetValidator DefaultDeskBudgetValidator()
    {
        return new DeskBudgetValidator(
            new List<IDeskBudgetRule>
            {
                new DeskBudgetRequiredCodeRule("desk budget needs a value"),
                new DeskBudgetCodePrefixRule("X"),
                new DeskBudgetRequiredContactEmailRule("desk budget needs a value"),
            });
    }
}
