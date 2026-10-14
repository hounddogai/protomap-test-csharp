namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a vendor digest must pass. Check returns its problems, or none.</summary>
public interface IVendorDigestRule
{
    List<string> Check(VendorDigest item);
}

public sealed record VendorDigestAllowedNameRule(List<string> Allowed) : IVendorDigestRule
{
    public List<string> Check(VendorDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record VendorDigestRequiredCodeRule(string Message) : IVendorDigestRule
{
    public List<string> Check(VendorDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorDigestCodePrefixRule(string Prefix) : IVendorDigestRule
{
    public List<string> Check(VendorDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor digest.</summary>
public sealed record VendorDigestValidator(List<IVendorDigestRule> Rules)
{
    public List<string> Validate(VendorDigest item)
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

    public void RequireValid(VendorDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorDigest> items)
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

/// <summary>Functions of vendor digest rules.</summary>
public static class VendorDigestRules
{
    public static VendorDigestValidator DefaultVendorDigestValidator()
    {
        return new VendorDigestValidator(
            new List<IVendorDigestRule>
            {
                new VendorDigestAllowedNameRule(new List<string>()),
                new VendorDigestRequiredCodeRule("vendor digest needs an entry"),
                new VendorDigestCodePrefixRule("REF"),
            });
    }
}
