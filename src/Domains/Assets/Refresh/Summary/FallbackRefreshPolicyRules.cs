namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback refresh policy must pass. Check returns its problems, or none.</summary>
public interface IFallbackRefreshPolicyRule
{
    List<string> Check(FallbackRefreshPolicy item);
}

public sealed record FallbackRefreshPolicyLabelLengthRule(int MaxLength) : IFallbackRefreshPolicyRule
{
    public List<string> Check(FallbackRefreshPolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRefreshPolicyAllowedCostCenterRule(List<string> Allowed) : IFallbackRefreshPolicyRule
{
    public List<string> Check(FallbackRefreshPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRefreshPolicyCodePrefixRule(string Prefix) : IFallbackRefreshPolicyRule
{
    public List<string> Check(FallbackRefreshPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRefreshPolicyRequiredSourceRule(string Message) : IFallbackRefreshPolicyRule
{
    public List<string> Check(FallbackRefreshPolicy item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRefreshPolicyRequiredCodeRule(string Message) : IFallbackRefreshPolicyRule
{
    public List<string> Check(FallbackRefreshPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRefreshPolicyRequiredLabelRule(string Message) : IFallbackRefreshPolicyRule
{
    public List<string> Check(FallbackRefreshPolicy item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback refresh policy.</summary>
public sealed record FallbackRefreshPolicyValidator(List<IFallbackRefreshPolicyRule> Rules)
{
    public List<string> Validate(FallbackRefreshPolicy item)
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

    public void RequireValid(FallbackRefreshPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackRefreshPolicy> items)
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

/// <summary>Functions of fallback refresh policy rules.</summary>
public static class FallbackRefreshPolicyRules
{
    public static FallbackRefreshPolicyValidator DefaultFallbackRefreshPolicyValidator()
    {
        return new FallbackRefreshPolicyValidator(
            new List<IFallbackRefreshPolicyRule>
            {
                new FallbackRefreshPolicyLabelLengthRule(16),
                new FallbackRefreshPolicyAllowedCostCenterRule(new List<string>()),
                new FallbackRefreshPolicyCodePrefixRule("OPS"),
                new FallbackRefreshPolicyRequiredSourceRule("fallback refresh policy needs an entry"),
                new FallbackRefreshPolicyRequiredCodeRule("fallback refresh policy needs an entry"),
                new FallbackRefreshPolicyRequiredLabelRule("fallback refresh policy needs this field"),
            });
    }
}
