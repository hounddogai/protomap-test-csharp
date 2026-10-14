namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack baseline must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckBaselineRule
{
    List<string> Check(PolicyAckBaseline item);
}

public sealed record PolicyAckBaselineSegmentLengthRule(int MaxLength) : IPolicyAckBaselineRule
{
    public List<string> Check(PolicyAckBaseline item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBaselineRequiredTitleRule(string Message) : IPolicyAckBaselineRule
{
    public List<string> Check(PolicyAckBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBaselineAllowedCodeRule(List<string> Allowed) : IPolicyAckBaselineRule
{
    public List<string> Check(PolicyAckBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBaselineDiscountRangeRule(double Min, double Max) : IPolicyAckBaselineRule
{
    public List<string> Check(PolicyAckBaseline item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBaselineCodePrefixRule(string Prefix) : IPolicyAckBaselineRule
{
    public List<string> Check(PolicyAckBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBaselineRequiredSegmentRule(string Message) : IPolicyAckBaselineRule
{
    public List<string> Check(PolicyAckBaseline item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack baseline.</summary>
public sealed record PolicyAckBaselineValidator(List<IPolicyAckBaselineRule> Rules)
{
    public List<string> Validate(PolicyAckBaseline item)
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

    public void RequireValid(PolicyAckBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckBaseline> items)
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

/// <summary>Functions of policy ack baseline rules.</summary>
public static class PolicyAckBaselineRules
{
    public static PolicyAckBaselineValidator DefaultPolicyAckBaselineValidator()
    {
        return new PolicyAckBaselineValidator(
            new List<IPolicyAckBaselineRule>
            {
                new PolicyAckBaselineSegmentLengthRule(255),
                new PolicyAckBaselineRequiredTitleRule("policy ack baseline needs a value"),
                new PolicyAckBaselineAllowedCodeRule(new List<string> { "manual", "regional", "scheduled" }),
                new PolicyAckBaselineDiscountRangeRule(4.8, 429.9),
                new PolicyAckBaselineCodePrefixRule("REF"),
                new PolicyAckBaselineRequiredSegmentRule("policy ack baseline needs this field"),
            });
    }
}
