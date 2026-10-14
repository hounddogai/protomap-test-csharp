namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack item must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckItemRule
{
    List<string> Check(PolicyAckItem item);
}

public sealed record PolicyAckItemCodePrefixRule(string Prefix) : IPolicyAckItemRule
{
    public List<string> Check(PolicyAckItem item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckItemAllowedCodeRule(List<string> Allowed) : IPolicyAckItemRule
{
    public List<string> Check(PolicyAckItem item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckItemRequiredLabelRule(string Message) : IPolicyAckItemRule
{
    public List<string> Check(PolicyAckItem item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckItemRateRangeRule(double Min, double Max) : IPolicyAckItemRule
{
    public List<string> Check(PolicyAckItem item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckItemSequenceRangeRule(int Min, int Max) : IPolicyAckItemRule
{
    public List<string> Check(PolicyAckItem item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckItemRequiredCodeRule(string Message) : IPolicyAckItemRule
{
    public List<string> Check(PolicyAckItem item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack item.</summary>
public sealed record PolicyAckItemValidator(List<IPolicyAckItemRule> Rules)
{
    public List<string> Validate(PolicyAckItem item)
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

    public void RequireValid(PolicyAckItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckItem> items)
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

/// <summary>Functions of policy ack item rules.</summary>
public static class PolicyAckItemRules
{
    public static PolicyAckItemValidator DefaultPolicyAckItemValidator()
    {
        return new PolicyAckItemValidator(
            new List<IPolicyAckItemRule>
            {
                new PolicyAckItemCodePrefixRule("X"),
                new PolicyAckItemAllowedCodeRule(new List<string> { "primary", "regional", "global" }),
                new PolicyAckItemRequiredLabelRule("policy ack item needs a value"),
                new PolicyAckItemRateRangeRule(8.4, 1240.1),
                new PolicyAckItemSequenceRangeRule(80, 86400),
                new PolicyAckItemRequiredCodeRule("policy ack item needs a value"),
            });
    }
}
