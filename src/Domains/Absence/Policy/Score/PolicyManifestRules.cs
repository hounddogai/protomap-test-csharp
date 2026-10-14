namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy manifest must pass. Check returns its problems, or none.</summary>
public interface IPolicyManifestRule
{
    List<string> Check(PolicyManifest item);
}

public sealed record PolicyManifestLimitRangeRule(int Min, int Max) : IPolicyManifestRule
{
    public List<string> Check(PolicyManifest item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyManifestCodePrefixRule(string Prefix) : IPolicyManifestRule
{
    public List<string> Check(PolicyManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyManifestEndsAtRangeRule(int Min, int Max) : IPolicyManifestRule
{
    public List<string> Check(PolicyManifest item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyManifestCodeLengthRule(int MaxLength) : IPolicyManifestRule
{
    public List<string> Check(PolicyManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyManifestAllowedCategoryRule(List<string> Allowed) : IPolicyManifestRule
{
    public List<string> Check(PolicyManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy manifest.</summary>
public sealed record PolicyManifestValidator(List<IPolicyManifestRule> Rules)
{
    public List<string> Validate(PolicyManifest item)
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

    public void RequireValid(PolicyManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyManifest> items)
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

/// <summary>Functions of policy manifest rules.</summary>
public static class PolicyManifestRules
{
    public static PolicyManifestValidator DefaultPolicyManifestValidator()
    {
        return new PolicyManifestValidator(
            new List<IPolicyManifestRule>
            {
                new PolicyManifestLimitRangeRule(32, 32),
                new PolicyManifestCodePrefixRule("OPS"),
                new PolicyManifestEndsAtRangeRule(16, 80),
                new PolicyManifestCodeLengthRule(86400),
                new PolicyManifestAllowedCategoryRule(new List<string> { "legacy", "capped", "adaptive" }),
            });
    }
}
