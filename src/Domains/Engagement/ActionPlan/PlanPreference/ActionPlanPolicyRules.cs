namespace Acme.Workforce.Engagement.ActionPlan.PlanPreference;

/// <summary>A rule a action plan policy must pass. Check returns its problems, or none.</summary>
public interface IActionPlanPolicyRule
{
    List<string> Check(ActionPlanPolicy item);
}

public sealed record ActionPlanPolicyNameLengthRule(int MaxLength) : IActionPlanPolicyRule
{
    public List<string> Check(ActionPlanPolicy item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanPolicyCodeLengthRule(int MaxLength) : IActionPlanPolicyRule
{
    public List<string> Check(ActionPlanPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanPolicyCodePrefixRule(string Prefix) : IActionPlanPolicyRule
{
    public List<string> Check(ActionPlanPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan policy.</summary>
public sealed record ActionPlanPolicyValidator(List<IActionPlanPolicyRule> Rules)
{
    public List<string> Validate(ActionPlanPolicy item)
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

    public void RequireValid(ActionPlanPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanPolicy> items)
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

/// <summary>Functions of action plan policy rules.</summary>
public static class ActionPlanPolicyRules
{
    public static ActionPlanPolicyValidator DefaultActionPlanPolicyValidator()
    {
        return new ActionPlanPolicyValidator(
            new List<IActionPlanPolicyRule>
            {
                new ActionPlanPolicyNameLengthRule(64),
                new ActionPlanPolicyCodeLengthRule(255),
                new ActionPlanPolicyCodePrefixRule("ACM"),
            });
    }
}
