namespace Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>A rule a policy ack cycle must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckCycleRule
{
    List<string> Check(PolicyAckCycle item);
}

public sealed record PolicyAckCycleNameLengthRule(int MaxLength) : IPolicyAckCycleRule
{
    public List<string> Check(PolicyAckCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckCycleCodePrefixRule(string Prefix) : IPolicyAckCycleRule
{
    public List<string> Check(PolicyAckCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckCycleSalaryBandLengthRule(int MaxLength) : IPolicyAckCycleRule
{
    public List<string> Check(PolicyAckCycle item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckCycleCodeLengthRule(int MaxLength) : IPolicyAckCycleRule
{
    public List<string> Check(PolicyAckCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckCycleRequiredRegionRule(string Message) : IPolicyAckCycleRule
{
    public List<string> Check(PolicyAckCycle item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckCycleRequiredNameRule(string Message) : IPolicyAckCycleRule
{
    public List<string> Check(PolicyAckCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack cycle.</summary>
public sealed record PolicyAckCycleValidator(List<IPolicyAckCycleRule> Rules)
{
    public List<string> Validate(PolicyAckCycle item)
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

    public void RequireValid(PolicyAckCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckCycle> items)
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

/// <summary>Functions of policy ack cycle rules.</summary>
public static class PolicyAckCycleRules
{
    public static PolicyAckCycleValidator DefaultPolicyAckCycleValidator()
    {
        return new PolicyAckCycleValidator(
            new List<IPolicyAckCycleRule>
            {
                new PolicyAckCycleNameLengthRule(64),
                new PolicyAckCycleCodePrefixRule("REF"),
                new PolicyAckCycleSalaryBandLengthRule(16),
                new PolicyAckCycleCodeLengthRule(32),
                new PolicyAckCycleRequiredRegionRule("policy ack cycle needs a value"),
                new PolicyAckCycleRequiredNameRule("policy ack cycle needs an entry"),
            });
    }
}
