namespace Acme.Workforce.Compliance.PolicyAck.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack plan must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckPlanRule
{
    List<string> Check(PolicyAckPlan item);
}

public sealed record PolicyAckPlanTitleLengthRule(int MaxLength) : IPolicyAckPlanRule
{
    public List<string> Check(PolicyAckPlan item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckPlanCodeLengthRule(int MaxLength) : IPolicyAckPlanRule
{
    public List<string> Check(PolicyAckPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckPlanCodePrefixRule(string Prefix) : IPolicyAckPlanRule
{
    public List<string> Check(PolicyAckPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckPlanRequiredOwnerIdRule(string Message) : IPolicyAckPlanRule
{
    public List<string> Check(PolicyAckPlan item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckPlanAllowedLocationRule(List<string> Allowed) : IPolicyAckPlanRule
{
    public List<string> Check(PolicyAckPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckPlanBudgetRangeRule(double Min, double Max) : IPolicyAckPlanRule
{
    public List<string> Check(PolicyAckPlan item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a policy ack plan.</summary>
public sealed record PolicyAckPlanValidator(List<IPolicyAckPlanRule> Rules)
{
    public List<string> Validate(PolicyAckPlan item)
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

    public void RequireValid(PolicyAckPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckPlan> items)
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

/// <summary>Functions of policy ack plan rules.</summary>
public static class PolicyAckPlanRules
{
    public static PolicyAckPlanValidator DefaultPolicyAckPlanValidator()
    {
        return new PolicyAckPlanValidator(
            new List<IPolicyAckPlanRule>
            {
                new PolicyAckPlanTitleLengthRule(32),
                new PolicyAckPlanCodeLengthRule(64),
                new PolicyAckPlanCodePrefixRule("REF"),
                new PolicyAckPlanRequiredOwnerIdRule("policy ack plan needs this field"),
                new PolicyAckPlanAllowedLocationRule(new List<string> { "manual", "primary" }),
                new PolicyAckPlanBudgetRangeRule(0.2, 4033.3),
            });
    }
}
