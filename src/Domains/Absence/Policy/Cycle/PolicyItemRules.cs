namespace Acme.Workforce.Absence.Policy.Cycle;

/// <summary>A rule a policy item must pass. Check returns its problems, or none.</summary>
public interface IPolicyItemRule
{
    List<string> Check(PolicyItem item);
}

public sealed record PolicyItemRequiredCodeRule(string Message) : IPolicyItemRule
{
    public List<string> Check(PolicyItem item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyItemTeamLengthRule(int MaxLength) : IPolicyItemRule
{
    public List<string> Check(PolicyItem item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyItemCurrencyLengthRule(int MaxLength) : IPolicyItemRule
{
    public List<string> Check(PolicyItem item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyItemLabelLengthRule(int MaxLength) : IPolicyItemRule
{
    public List<string> Check(PolicyItem item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyItemUpdatedAtRangeRule(int Min, int Max) : IPolicyItemRule
{
    public List<string> Check(PolicyItem item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyItemRequiredTeamRule(string Message) : IPolicyItemRule
{
    public List<string> Check(PolicyItem item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy item.</summary>
public sealed record PolicyItemValidator(List<IPolicyItemRule> Rules)
{
    public List<string> Validate(PolicyItem item)
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

    public void RequireValid(PolicyItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyItem> items)
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

/// <summary>Functions of policy item rules.</summary>
public static class PolicyItemRules
{
    public static PolicyItemValidator DefaultPolicyItemValidator()
    {
        return new PolicyItemValidator(
            new List<IPolicyItemRule>
            {
                new PolicyItemRequiredCodeRule("policy item needs this field"),
                new PolicyItemTeamLengthRule(16),
                new PolicyItemCurrencyLengthRule(3600),
                new PolicyItemLabelLengthRule(3600),
                new PolicyItemUpdatedAtRangeRule(64, 80),
                new PolicyItemRequiredTeamRule("policy item needs this field"),
            });
    }
}
