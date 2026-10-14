namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack adjustment must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckAdjustmentRule
{
    List<string> Check(PolicyAckAdjustment item);
}

public sealed record PolicyAckAdjustmentLimitRangeRule(int Min, int Max) : IPolicyAckAdjustmentRule
{
    public List<string> Check(PolicyAckAdjustment item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyAckAdjustmentRequiredTitleRule(string Message) : IPolicyAckAdjustmentRule
{
    public List<string> Check(PolicyAckAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAdjustmentAllowedOwnerIdRule(List<string> Allowed) : IPolicyAckAdjustmentRule
{
    public List<string> Check(PolicyAckAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack adjustment.</summary>
public sealed record PolicyAckAdjustmentValidator(List<IPolicyAckAdjustmentRule> Rules)
{
    public List<string> Validate(PolicyAckAdjustment item)
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

    public void RequireValid(PolicyAckAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckAdjustment> items)
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

/// <summary>Functions of policy ack adjustment rules.</summary>
public static class PolicyAckAdjustmentRules
{
    public static PolicyAckAdjustmentValidator DefaultPolicyAckAdjustmentValidator()
    {
        return new PolicyAckAdjustmentValidator(
            new List<IPolicyAckAdjustmentRule>
            {
                new PolicyAckAdjustmentLimitRangeRule(3600, 86400),
                new PolicyAckAdjustmentRequiredTitleRule("policy ack adjustment needs an entry"),
                new PolicyAckAdjustmentAllowedOwnerIdRule(new List<string> { "fallback", "global", "seasonal" }),
            });
    }
}
