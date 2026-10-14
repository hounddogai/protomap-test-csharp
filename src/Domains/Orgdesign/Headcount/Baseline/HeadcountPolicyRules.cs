namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount policy must pass. Check returns its problems, or none.</summary>
public interface IHeadcountPolicyRule
{
    List<string> Check(HeadcountPolicy item);
}

public sealed record HeadcountPolicyCostRangeRule(double Min, double Max) : IHeadcountPolicyRule
{
    public List<string> Check(HeadcountPolicy item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountPolicyAllowedLabelRule(List<string> Allowed) : IHeadcountPolicyRule
{
    public List<string> Check(HeadcountPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountPolicyRequiredChannelRule(string Message) : IHeadcountPolicyRule
{
    public List<string> Check(HeadcountPolicy item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountPolicyLabelLengthRule(int MaxLength) : IHeadcountPolicyRule
{
    public List<string> Check(HeadcountPolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountPolicyExpiresAtRangeRule(int Min, int Max) : IHeadcountPolicyRule
{
    public List<string> Check(HeadcountPolicy item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountPolicyCodePrefixRule(string Prefix) : IHeadcountPolicyRule
{
    public List<string> Check(HeadcountPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount policy.</summary>
public sealed record HeadcountPolicyValidator(List<IHeadcountPolicyRule> Rules)
{
    public List<string> Validate(HeadcountPolicy item)
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

    public void RequireValid(HeadcountPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountPolicy> items)
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

/// <summary>Functions of headcount policy rules.</summary>
public static class HeadcountPolicyRules
{
    public static HeadcountPolicyValidator DefaultHeadcountPolicyValidator()
    {
        return new HeadcountPolicyValidator(
            new List<IHeadcountPolicyRule>
            {
                new HeadcountPolicyCostRangeRule(4.2, 1988.8),
                new HeadcountPolicyAllowedLabelRule(new List<string> { "batched", "pending" }),
                new HeadcountPolicyRequiredChannelRule("headcount policy needs this field"),
                new HeadcountPolicyLabelLengthRule(32),
                new HeadcountPolicyExpiresAtRangeRule(16, 120),
                new HeadcountPolicyCodePrefixRule("ACM"),
            });
    }
}
