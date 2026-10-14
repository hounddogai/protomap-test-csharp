namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack allocation must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckAllocationRule
{
    List<string> Check(PolicyAckAllocation item);
}

public sealed record PolicyAckAllocationOwnerIdLengthRule(int MaxLength) : IPolicyAckAllocationRule
{
    public List<string> Check(PolicyAckAllocation item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAllocationLimitRangeRule(int Min, int Max) : IPolicyAckAllocationRule
{
    public List<string> Check(PolicyAckAllocation item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAllocationRequiredGradeRule(string Message) : IPolicyAckAllocationRule
{
    public List<string> Check(PolicyAckAllocation item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAllocationPinnedGuardRule(int Limit) : IPolicyAckAllocationRule
{
    public List<string> Check(PolicyAckAllocation item)
    {
        if (item.Pinned && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAllocationCodePrefixRule(string Prefix) : IPolicyAckAllocationRule
{
    public List<string> Check(PolicyAckAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAllocationAllowedLabelRule(List<string> Allowed) : IPolicyAckAllocationRule
{
    public List<string> Check(PolicyAckAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack allocation.</summary>
public sealed record PolicyAckAllocationValidator(List<IPolicyAckAllocationRule> Rules)
{
    public List<string> Validate(PolicyAckAllocation item)
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

    public void RequireValid(PolicyAckAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckAllocation> items)
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

/// <summary>Functions of policy ack allocation rules.</summary>
public static class PolicyAckAllocationRules
{
    public static PolicyAckAllocationValidator DefaultPolicyAckAllocationValidator()
    {
        return new PolicyAckAllocationValidator(
            new List<IPolicyAckAllocationRule>
            {
                new PolicyAckAllocationOwnerIdLengthRule(16),
                new PolicyAckAllocationLimitRangeRule(120, 3600),
                new PolicyAckAllocationRequiredGradeRule("policy ack allocation needs an entry"),
                new PolicyAckAllocationPinnedGuardRule(255),
                new PolicyAckAllocationCodePrefixRule("OPS"),
                new PolicyAckAllocationAllowedLabelRule(new List<string> { "pending", "capped" }),
            });
    }
}
