namespace Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>A rule a policy budget must pass. Check returns its problems, or none.</summary>
public interface IPolicyBudgetRule
{
    List<string> Check(PolicyBudget item);
}

public sealed record PolicyBudgetRequiredDescriptionRule(string Message) : IPolicyBudgetRule
{
    public List<string> Check(PolicyBudget item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyBudgetRequiredNotesRule(string Message) : IPolicyBudgetRule
{
    public List<string> Check(PolicyBudget item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyBudgetRequiredCodeRule(string Message) : IPolicyBudgetRule
{
    public List<string> Check(PolicyBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy budget.</summary>
public sealed record PolicyBudgetValidator(List<IPolicyBudgetRule> Rules)
{
    public List<string> Validate(PolicyBudget item)
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

    public void RequireValid(PolicyBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyBudget> items)
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

/// <summary>Functions of policy budget rules.</summary>
public static class PolicyBudgetRules
{
    public static PolicyBudgetValidator DefaultPolicyBudgetValidator()
    {
        return new PolicyBudgetValidator(
            new List<IPolicyBudgetRule>
            {
                new PolicyBudgetRequiredDescriptionRule("policy budget needs an entry"),
                new PolicyBudgetRequiredNotesRule("policy budget needs a value"),
                new PolicyBudgetRequiredCodeRule("policy budget needs an entry"),
            });
    }
}
