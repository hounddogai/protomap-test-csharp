namespace Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>A rule a policy ack digest must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckDigestRule
{
    List<string> Check(PolicyAckDigest item);
}

public sealed record PolicyAckDigestCodePrefixRule(string Prefix) : IPolicyAckDigestRule
{
    public List<string> Check(PolicyAckDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckDigestRequiredSkillRule(string Message) : IPolicyAckDigestRule
{
    public List<string> Check(PolicyAckDigest item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckDigestRequiredTitleRule(string Message) : IPolicyAckDigestRule
{
    public List<string> Check(PolicyAckDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckDigestTitleLengthRule(int MaxLength) : IPolicyAckDigestRule
{
    public List<string> Check(PolicyAckDigest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckDigestRequiredCodeRule(string Message) : IPolicyAckDigestRule
{
    public List<string> Check(PolicyAckDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckDigestRatioRangeRule(double Min, double Max) : IPolicyAckDigestRule
{
    public List<string> Check(PolicyAckDigest item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack digest.</summary>
public sealed record PolicyAckDigestValidator(List<IPolicyAckDigestRule> Rules)
{
    public List<string> Validate(PolicyAckDigest item)
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

    public void RequireValid(PolicyAckDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckDigest> items)
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

/// <summary>Functions of policy ack digest rules.</summary>
public static class PolicyAckDigestRules
{
    public static PolicyAckDigestValidator DefaultPolicyAckDigestValidator()
    {
        return new PolicyAckDigestValidator(
            new List<IPolicyAckDigestRule>
            {
                new PolicyAckDigestCodePrefixRule("REF"),
                new PolicyAckDigestRequiredSkillRule("policy ack digest needs this field"),
                new PolicyAckDigestRequiredTitleRule("policy ack digest needs an entry"),
                new PolicyAckDigestTitleLengthRule(120),
                new PolicyAckDigestRequiredCodeRule("policy ack digest needs a value"),
                new PolicyAckDigestRatioRangeRule(9.7, 2696.1),
            });
    }
}
