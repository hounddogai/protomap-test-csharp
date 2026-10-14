namespace Acme.Workforce.Absence.Policy.Notice;

/// <summary>A rule a policy draft must pass. Check returns its problems, or none.</summary>
public interface IPolicyDraftRule
{
    List<string> Check(PolicyDraft item);
}

public sealed record PolicyDraftRequiredUnitRule(string Message) : IPolicyDraftRule
{
    public List<string> Check(PolicyDraft item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyDraftRequiredChannelRule(string Message) : IPolicyDraftRule
{
    public List<string> Check(PolicyDraft item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyDraftRequiredCodeRule(string Message) : IPolicyDraftRule
{
    public List<string> Check(PolicyDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy draft.</summary>
public sealed record PolicyDraftValidator(List<IPolicyDraftRule> Rules)
{
    public List<string> Validate(PolicyDraft item)
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

    public void RequireValid(PolicyDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyDraft> items)
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

/// <summary>Functions of policy draft rules.</summary>
public static class PolicyDraftRules
{
    public static PolicyDraftValidator DefaultPolicyDraftValidator()
    {
        return new PolicyDraftValidator(
            new List<IPolicyDraftRule>
            {
                new PolicyDraftRequiredUnitRule("policy draft needs an entry"),
                new PolicyDraftRequiredChannelRule("policy draft needs a value"),
                new PolicyDraftRequiredCodeRule("policy draft needs this field"),
            });
    }
}
