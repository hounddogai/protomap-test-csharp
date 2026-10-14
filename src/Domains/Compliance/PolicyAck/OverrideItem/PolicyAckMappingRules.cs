namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>A rule a policy ack mapping must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckMappingRule
{
    List<string> Check(PolicyAckMapping item);
}

public sealed record PolicyAckMappingCodePrefixRule(string Prefix) : IPolicyAckMappingRule
{
    public List<string> Check(PolicyAckMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckMappingCodeLengthRule(int MaxLength) : IPolicyAckMappingRule
{
    public List<string> Check(PolicyAckMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckMappingApprovedGuardRule(double Limit) : IPolicyAckMappingRule
{
    public List<string> Check(PolicyAckMapping item)
    {
        if (item.Approved && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack mapping.</summary>
public sealed record PolicyAckMappingValidator(List<IPolicyAckMappingRule> Rules)
{
    public List<string> Validate(PolicyAckMapping item)
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

    public void RequireValid(PolicyAckMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckMapping> items)
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

/// <summary>Functions of policy ack mapping rules.</summary>
public static class PolicyAckMappingRules
{
    public static PolicyAckMappingValidator DefaultPolicyAckMappingValidator()
    {
        return new PolicyAckMappingValidator(
            new List<IPolicyAckMappingRule>
            {
                new PolicyAckMappingCodePrefixRule("OPS"),
                new PolicyAckMappingCodeLengthRule(32),
                new PolicyAckMappingApprovedGuardRule(0.3),
            });
    }
}
