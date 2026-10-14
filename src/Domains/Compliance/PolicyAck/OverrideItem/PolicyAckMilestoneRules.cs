namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>A rule a policy ack milestone must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckMilestoneRule
{
    List<string> Check(PolicyAckMilestone item);
}

public sealed record PolicyAckMilestoneCapacityRangeRule(int Min, int Max) : IPolicyAckMilestoneRule
{
    public List<string> Check(PolicyAckMilestone item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckMilestoneCodePrefixRule(string Prefix) : IPolicyAckMilestoneRule
{
    public List<string> Check(PolicyAckMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckMilestoneRequiredDepartmentRule(string Message) : IPolicyAckMilestoneRule
{
    public List<string> Check(PolicyAckMilestone item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack milestone.</summary>
public sealed record PolicyAckMilestoneValidator(List<IPolicyAckMilestoneRule> Rules)
{
    public List<string> Validate(PolicyAckMilestone item)
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

    public void RequireValid(PolicyAckMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckMilestone> items)
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

/// <summary>Functions of policy ack milestone rules.</summary>
public static class PolicyAckMilestoneRules
{
    public static PolicyAckMilestoneValidator DefaultPolicyAckMilestoneValidator()
    {
        return new PolicyAckMilestoneValidator(
            new List<IPolicyAckMilestoneRule>
            {
                new PolicyAckMilestoneCapacityRangeRule(16, 80),
                new PolicyAckMilestoneCodePrefixRule("REF"),
                new PolicyAckMilestoneRequiredDepartmentRule("policy ack milestone needs a value"),
            });
    }
}
