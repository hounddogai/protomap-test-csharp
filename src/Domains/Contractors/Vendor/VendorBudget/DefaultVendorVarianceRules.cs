namespace Acme.Workforce.Contractors.Vendor.VendorBudget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default vendor variance must pass. Check returns its problems, or none.</summary>
public interface IDefaultVendorVarianceRule
{
    List<string> Check(DefaultVendorVariance item);
}

public sealed record DefaultVendorVarianceRequiredPhoneRule(string Message) : IDefaultVendorVarianceRule
{
    public List<string> Check(DefaultVendorVariance item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultVendorVarianceRequiredCategoryRule(string Message) : IDefaultVendorVarianceRule
{
    public List<string> Check(DefaultVendorVariance item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultVendorVarianceCodePrefixRule(string Prefix) : IDefaultVendorVarianceRule
{
    public List<string> Check(DefaultVendorVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultVendorVarianceAllowedCategoryRule(List<string> Allowed) : IDefaultVendorVarianceRule
{
    public List<string> Check(DefaultVendorVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default vendor variance.</summary>
public sealed record DefaultVendorVarianceValidator(List<IDefaultVendorVarianceRule> Rules)
{
    public List<string> Validate(DefaultVendorVariance item)
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

    public void RequireValid(DefaultVendorVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultVendorVariance> items)
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

/// <summary>Functions of default vendor variance rules.</summary>
public static class DefaultVendorVarianceRules
{
    public static DefaultVendorVarianceValidator DefaultDefaultVendorVarianceValidator()
    {
        return new DefaultVendorVarianceValidator(
            new List<IDefaultVendorVarianceRule>
            {
                new DefaultVendorVarianceRequiredPhoneRule("default vendor variance needs this field"),
                new DefaultVendorVarianceRequiredCategoryRule("default vendor variance needs an entry"),
                new DefaultVendorVarianceCodePrefixRule("ACM"),
                new DefaultVendorVarianceAllowedCategoryRule(new List<string> { "batched", "fallback", "adaptive" }),
            });
    }
}
