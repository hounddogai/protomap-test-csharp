namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy bundle must pass. Check returns its problems, or none.</summary>
public interface IPolicyBundleRule
{
    List<string> Check(PolicyBundle item);
}

public sealed record PolicyBundleAllowedCurrencyRule(List<string> Allowed) : IPolicyBundleRule
{
    public List<string> Check(PolicyBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is too long" };
        }
        return new List<string>();
    }
}

public sealed record PolicyBundleUnitLengthRule(int MaxLength) : IPolicyBundleRule
{
    public List<string> Check(PolicyBundle item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyBundleCostCenterLengthRule(int MaxLength) : IPolicyBundleRule
{
    public List<string> Check(PolicyBundle item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyBundleCodePrefixRule(string Prefix) : IPolicyBundleRule
{
    public List<string> Check(PolicyBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy bundle.</summary>
public sealed record PolicyBundleValidator(List<IPolicyBundleRule> Rules)
{
    public List<string> Validate(PolicyBundle item)
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

    public void RequireValid(PolicyBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyBundle> items)
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

/// <summary>Functions of policy bundle rules.</summary>
public static class PolicyBundleRules
{
    public static PolicyBundleValidator DefaultPolicyBundleValidator()
    {
        return new PolicyBundleValidator(
            new List<IPolicyBundleRule>
            {
                new PolicyBundleAllowedCurrencyRule(new List<string> { "default" }),
                new PolicyBundleUnitLengthRule(255),
                new PolicyBundleCostCenterLengthRule(64),
                new PolicyBundleCodePrefixRule("X"),
            });
    }
}
