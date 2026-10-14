namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>A rule a policy ack preference must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckPreferenceRule
{
    List<string> Check(PolicyAckPreference item);
}

public sealed record PolicyAckPreferenceCodeLengthRule(int MaxLength) : IPolicyAckPreferenceRule
{
    public List<string> Check(PolicyAckPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckPreferenceRequiredManagerIdRule(string Message) : IPolicyAckPreferenceRule
{
    public List<string> Check(PolicyAckPreference item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckPreferenceCodePrefixRule(string Prefix) : IPolicyAckPreferenceRule
{
    public List<string> Check(PolicyAckPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack preference.</summary>
public sealed record PolicyAckPreferenceValidator(List<IPolicyAckPreferenceRule> Rules)
{
    public List<string> Validate(PolicyAckPreference item)
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

    public void RequireValid(PolicyAckPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckPreference> items)
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

/// <summary>Functions of policy ack preference rules.</summary>
public static class PolicyAckPreferenceRules
{
    public static PolicyAckPreferenceValidator DefaultPolicyAckPreferenceValidator()
    {
        return new PolicyAckPreferenceValidator(
            new List<IPolicyAckPreferenceRule>
            {
                new PolicyAckPreferenceCodeLengthRule(64),
                new PolicyAckPreferenceRequiredManagerIdRule("policy ack preference needs this field"),
                new PolicyAckPreferenceCodePrefixRule("Q"),
            });
    }
}
