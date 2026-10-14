namespace Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>A rule a vendor baseline must pass. Check returns its problems, or none.</summary>
public interface IVendorBaselineRule
{
    List<string> Check(VendorBaseline item);
}

public sealed record VendorBaselineRequiredCodeRule(string Message) : IVendorBaselineRule
{
    public List<string> Check(VendorBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorBaselineTagsLimitRule(int MaxCount, List<string> Banned) : IVendorBaselineRule
{
    public List<string> Check(VendorBaseline item)
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

public sealed record VendorBaselineRequiredTitleRule(string Message) : IVendorBaselineRule
{
    public List<string> Check(VendorBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor baseline.</summary>
public sealed record VendorBaselineValidator(List<IVendorBaselineRule> Rules)
{
    public List<string> Validate(VendorBaseline item)
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

    public void RequireValid(VendorBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorBaseline> items)
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

/// <summary>Functions of vendor baseline rules.</summary>
public static class VendorBaselineRules
{
    public static VendorBaselineValidator DefaultVendorBaselineValidator()
    {
        return new VendorBaselineValidator(
            new List<IVendorBaselineRule>
            {
                new VendorBaselineRequiredCodeRule("vendor baseline needs this field"),
                new VendorBaselineTagsLimitRule(1000, new List<string> { "lenient", "seasonal", "legacy" }),
                new VendorBaselineRequiredTitleRule("vendor baseline needs an entry"),
            });
    }
}
