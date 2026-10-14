namespace Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>A rule a pay band policy must pass. Check returns its problems, or none.</summary>
public interface IPayBandPolicyRule
{
    List<string> Check(PayBandPolicy item);
}

public sealed record PayBandPolicyRequiredCodeRule(string Message) : IPayBandPolicyRule
{
    public List<string> Check(PayBandPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandPolicyRateRangeRule(double Min, double Max) : IPayBandPolicyRule
{
    public List<string> Check(PayBandPolicy item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandPolicyCodePrefixRule(string Prefix) : IPayBandPolicyRule
{
    public List<string> Check(PayBandPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandPolicyTagsLimitRule(int MaxCount, List<string> Banned) : IPayBandPolicyRule
{
    public List<string> Check(PayBandPolicy item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pay band policy.</summary>
public sealed record PayBandPolicyValidator(List<IPayBandPolicyRule> Rules)
{
    public List<string> Validate(PayBandPolicy item)
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

    public void RequireValid(PayBandPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandPolicy> items)
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

/// <summary>Functions of pay band policy rules.</summary>
public static class PayBandPolicyRules
{
    public static PayBandPolicyValidator DefaultPayBandPolicyValidator()
    {
        return new PayBandPolicyValidator(
            new List<IPayBandPolicyRule>
            {
                new PayBandPolicyRequiredCodeRule("pay band policy needs this field"),
                new PayBandPolicyRateRangeRule(5.4, 1798.1),
                new PayBandPolicyCodePrefixRule("ACM"),
                new PayBandPolicyTagsLimitRule(120, new List<string> { "global" }),
            });
    }
}
