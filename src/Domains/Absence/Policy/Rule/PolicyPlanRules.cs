namespace Acme.Workforce.Absence.Policy.Rule;

/// <summary>A rule a policy plan must pass. Check returns its problems, or none.</summary>
public interface IPolicyPlanRule
{
    List<string> Check(PolicyPlan item);
}

public sealed record PolicyPlanRequiredCodeRule(string Message) : IPolicyPlanRule
{
    public List<string> Check(PolicyPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyPlanRequiredBadgeRule(string Message) : IPolicyPlanRule
{
    public List<string> Check(PolicyPlan item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyPlanLabelLengthRule(int MaxLength) : IPolicyPlanRule
{
    public List<string> Check(PolicyPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyPlanCodeLengthRule(int MaxLength) : IPolicyPlanRule
{
    public List<string> Check(PolicyPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyPlanRetriesRangeRule(int Min, int Max) : IPolicyPlanRule
{
    public List<string> Check(PolicyPlan item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyPlanRequiredLabelRule(string Message) : IPolicyPlanRule
{
    public List<string> Check(PolicyPlan item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy plan.</summary>
public sealed record PolicyPlanValidator(List<IPolicyPlanRule> Rules)
{
    public List<string> Validate(PolicyPlan item)
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

    public void RequireValid(PolicyPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyPlan> items)
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

/// <summary>Functions of policy plan rules.</summary>
public static class PolicyPlanRules
{
    public static PolicyPlanValidator DefaultPolicyPlanValidator()
    {
        return new PolicyPlanValidator(
            new List<IPolicyPlanRule>
            {
                new PolicyPlanRequiredCodeRule("policy plan needs a value"),
                new PolicyPlanRequiredBadgeRule("policy plan needs this field"),
                new PolicyPlanLabelLengthRule(255),
                new PolicyPlanCodeLengthRule(255),
                new PolicyPlanRetriesRangeRule(16, 80),
                new PolicyPlanRequiredLabelRule("policy plan needs this field"),
            });
    }
}
