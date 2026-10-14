namespace Acme.Workforce.Compliance.PolicyAck.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack rollup must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckRollupRule
{
    List<string> Check(PolicyAckRollup item);
}

public sealed record PolicyAckRollupThresholdRangeRule(double Min, double Max) : IPolicyAckRollupRule
{
    public List<string> Check(PolicyAckRollup item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckRollupCodeLengthRule(int MaxLength) : IPolicyAckRollupRule
{
    public List<string> Check(PolicyAckRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckRollupAllowedNameRule(List<string> Allowed) : IPolicyAckRollupRule
{
    public List<string> Check(PolicyAckRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckRollupCodePrefixRule(string Prefix) : IPolicyAckRollupRule
{
    public List<string> Check(PolicyAckRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckRollupFloorValueRangeRule(double Min, double Max) : IPolicyAckRollupRule
{
    public List<string> Check(PolicyAckRollup item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckRollupNameLengthRule(int MaxLength) : IPolicyAckRollupRule
{
    public List<string> Check(PolicyAckRollup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack rollup.</summary>
public sealed record PolicyAckRollupValidator(List<IPolicyAckRollupRule> Rules)
{
    public List<string> Validate(PolicyAckRollup item)
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

    public void RequireValid(PolicyAckRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckRollup> items)
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

/// <summary>Functions of policy ack rollup rules.</summary>
public static class PolicyAckRollupRules
{
    public static PolicyAckRollupValidator DefaultPolicyAckRollupValidator()
    {
        return new PolicyAckRollupValidator(
            new List<IPolicyAckRollupRule>
            {
                new PolicyAckRollupThresholdRangeRule(5.5, 3837.9),
                new PolicyAckRollupCodeLengthRule(120),
                new PolicyAckRollupAllowedNameRule(new List<string> { "scheduled", "default" }),
                new PolicyAckRollupCodePrefixRule("REF"),
                new PolicyAckRollupFloorValueRangeRule(5.0, 3857.6),
                new PolicyAckRollupNameLengthRule(80),
            });
    }
}
