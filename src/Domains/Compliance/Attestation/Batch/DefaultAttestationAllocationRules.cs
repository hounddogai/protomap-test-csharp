namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default attestation allocation must pass. Check returns its problems, or none.</summary>
public interface IDefaultAttestationAllocationRule
{
    List<string> Check(DefaultAttestationAllocation item);
}

public sealed record DefaultAttestationAllocationAllowedTitleRule(List<string> Allowed) : IDefaultAttestationAllocationRule
{
    public List<string> Check(DefaultAttestationAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAttestationAllocationRequiredManagerIdRule(string Message) : IDefaultAttestationAllocationRule
{
    public List<string> Check(DefaultAttestationAllocation item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAttestationAllocationRequiredBadgeRule(string Message) : IDefaultAttestationAllocationRule
{
    public List<string> Check(DefaultAttestationAllocation item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAttestationAllocationCodePrefixRule(string Prefix) : IDefaultAttestationAllocationRule
{
    public List<string> Check(DefaultAttestationAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default attestation allocation.</summary>
public sealed record DefaultAttestationAllocationValidator(List<IDefaultAttestationAllocationRule> Rules)
{
    public List<string> Validate(DefaultAttestationAllocation item)
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

    public void RequireValid(DefaultAttestationAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAttestationAllocation> items)
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

/// <summary>Functions of default attestation allocation rules.</summary>
public static class DefaultAttestationAllocationRules
{
    public static DefaultAttestationAllocationValidator DefaultDefaultAttestationAllocationValidator()
    {
        return new DefaultAttestationAllocationValidator(
            new List<IDefaultAttestationAllocationRule>
            {
                new DefaultAttestationAllocationAllowedTitleRule(new List<string> { "batched" }),
                new DefaultAttestationAllocationRequiredManagerIdRule(
                    "default attestation allocation needs this field"),
                new DefaultAttestationAllocationRequiredBadgeRule("default attestation allocation needs this field"),
                new DefaultAttestationAllocationCodePrefixRule("X"),
            });
    }
}
