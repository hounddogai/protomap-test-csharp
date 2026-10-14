namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

/// <summary>A rule a policy ack assignment must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckAssignmentRule
{
    List<string> Check(PolicyAckAssignment item);
}

public sealed record PolicyAckAssignmentRequiredTitleRule(string Message) : IPolicyAckAssignmentRule
{
    public List<string> Check(PolicyAckAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAssignmentLocaleLengthRule(int MaxLength) : IPolicyAckAssignmentRule
{
    public List<string> Check(PolicyAckAssignment item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAssignmentRatioRangeRule(double Min, double Max) : IPolicyAckAssignmentRule
{
    public List<string> Check(PolicyAckAssignment item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAssignmentTitleLengthRule(int MaxLength) : IPolicyAckAssignmentRule
{
    public List<string> Check(PolicyAckAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAssignmentCodePrefixRule(string Prefix) : IPolicyAckAssignmentRule
{
    public List<string> Check(PolicyAckAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack assignment.</summary>
public sealed record PolicyAckAssignmentValidator(List<IPolicyAckAssignmentRule> Rules)
{
    public List<string> Validate(PolicyAckAssignment item)
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

    public void RequireValid(PolicyAckAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckAssignment> items)
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

/// <summary>Functions of policy ack assignment rules.</summary>
public static class PolicyAckAssignmentRules
{
    public static PolicyAckAssignmentValidator DefaultPolicyAckAssignmentValidator()
    {
        return new PolicyAckAssignmentValidator(
            new List<IPolicyAckAssignmentRule>
            {
                new PolicyAckAssignmentRequiredTitleRule("policy ack assignment needs an entry"),
                new PolicyAckAssignmentLocaleLengthRule(64),
                new PolicyAckAssignmentRatioRangeRule(1.2, 2635.9),
                new PolicyAckAssignmentTitleLengthRule(64),
                new PolicyAckAssignmentCodePrefixRule("ACM"),
            });
    }
}
