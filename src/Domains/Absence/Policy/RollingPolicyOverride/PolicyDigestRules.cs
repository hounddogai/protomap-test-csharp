namespace Acme.Workforce.Absence.Policy.RollingPolicyOverride;

/// <summary>A rule a policy digest must pass. Check returns its problems, or none.</summary>
public interface IPolicyDigestRule
{
    List<string> Check(PolicyDigest item);
}

public sealed record PolicyDigestOwnerIdLengthRule(int MaxLength) : IPolicyDigestRule
{
    public List<string> Check(PolicyDigest item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyDigestCodePrefixRule(string Prefix) : IPolicyDigestRule
{
    public List<string> Check(PolicyDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyDigestRequiredRegionRule(string Message) : IPolicyDigestRule
{
    public List<string> Check(PolicyDigest item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyDigestRequiredNameRule(string Message) : IPolicyDigestRule
{
    public List<string> Check(PolicyDigest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyDigestNameLengthRule(int MaxLength) : IPolicyDigestRule
{
    public List<string> Check(PolicyDigest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy digest.</summary>
public sealed record PolicyDigestValidator(List<IPolicyDigestRule> Rules)
{
    public List<string> Validate(PolicyDigest item)
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

    public void RequireValid(PolicyDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyDigest> items)
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

/// <summary>Functions of policy digest rules.</summary>
public static class PolicyDigestRules
{
    public static PolicyDigestValidator DefaultPolicyDigestValidator()
    {
        return new PolicyDigestValidator(
            new List<IPolicyDigestRule>
            {
                new PolicyDigestOwnerIdLengthRule(3600),
                new PolicyDigestCodePrefixRule("REF"),
                new PolicyDigestRequiredRegionRule("policy digest needs a value"),
                new PolicyDigestRequiredNameRule("policy digest needs this field"),
                new PolicyDigestNameLengthRule(3600),
            });
    }
}
