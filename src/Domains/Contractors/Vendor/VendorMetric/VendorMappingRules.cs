namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a vendor mapping must pass. Check returns its problems, or none.</summary>
public interface IVendorMappingRule
{
    List<string> Check(VendorMapping item);
}

public sealed record VendorMappingRequiredCodeRule(string Message) : IVendorMappingRule
{
    public List<string> Check(VendorMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorMappingSkillLengthRule(int MaxLength) : IVendorMappingRule
{
    public List<string> Check(VendorMapping item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorMappingAllowedSkillRule(List<string> Allowed) : IVendorMappingRule
{
    public List<string> Check(VendorMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record VendorMappingCodePrefixRule(string Prefix) : IVendorMappingRule
{
    public List<string> Check(VendorMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VendorMappingRequiredSkillRule(string Message) : IVendorMappingRule
{
    public List<string> Check(VendorMapping item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorMappingTitleLengthRule(int MaxLength) : IVendorMappingRule
{
    public List<string> Check(VendorMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor mapping.</summary>
public sealed record VendorMappingValidator(List<IVendorMappingRule> Rules)
{
    public List<string> Validate(VendorMapping item)
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

    public void RequireValid(VendorMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorMapping> items)
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

/// <summary>Functions of vendor mapping rules.</summary>
public static class VendorMappingRules
{
    public static VendorMappingValidator DefaultVendorMappingValidator()
    {
        return new VendorMappingValidator(
            new List<IVendorMappingRule>
            {
                new VendorMappingRequiredCodeRule("vendor mapping needs a value"),
                new VendorMappingSkillLengthRule(16),
                new VendorMappingAllowedSkillRule(new List<string> { "default", "global", "global" }),
                new VendorMappingCodePrefixRule("ACM"),
                new VendorMappingRequiredSkillRule("vendor mapping needs this field"),
                new VendorMappingTitleLengthRule(32),
            });
    }
}
