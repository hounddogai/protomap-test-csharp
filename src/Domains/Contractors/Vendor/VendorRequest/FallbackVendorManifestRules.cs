namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

/// <summary>A rule a fallback vendor manifest must pass. Check returns its problems, or none.</summary>
public interface IFallbackVendorManifestRule
{
    List<string> Check(FallbackVendorManifest item);
}

public sealed record FallbackVendorManifestCodePrefixRule(string Prefix) : IFallbackVendorManifestRule
{
    public List<string> Check(FallbackVendorManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackVendorManifestRequiredCodeRule(string Message) : IFallbackVendorManifestRule
{
    public List<string> Check(FallbackVendorManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackVendorManifestTagsLimitRule(int MaxCount, List<string> Banned) : IFallbackVendorManifestRule
{
    public List<string> Check(FallbackVendorManifest item)
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

public sealed record FallbackVendorManifestRequiredNameRule(string Message) : IFallbackVendorManifestRule
{
    public List<string> Check(FallbackVendorManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackVendorManifestOwnerIdLengthRule(int MaxLength) : IFallbackVendorManifestRule
{
    public List<string> Check(FallbackVendorManifest item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackVendorManifestNameLengthRule(int MaxLength) : IFallbackVendorManifestRule
{
    public List<string> Check(FallbackVendorManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback vendor manifest.</summary>
public sealed record FallbackVendorManifestValidator(List<IFallbackVendorManifestRule> Rules)
{
    public List<string> Validate(FallbackVendorManifest item)
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

    public void RequireValid(FallbackVendorManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackVendorManifest> items)
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

/// <summary>Functions of fallback vendor manifest rules.</summary>
public static class FallbackVendorManifestRules
{
    public static FallbackVendorManifestValidator DefaultFallbackVendorManifestValidator()
    {
        return new FallbackVendorManifestValidator(
            new List<IFallbackVendorManifestRule>
            {
                new FallbackVendorManifestCodePrefixRule("Q"),
                new FallbackVendorManifestRequiredCodeRule("fallback vendor manifest needs an entry"),
                new FallbackVendorManifestTagsLimitRule(3600, new List<string> { "regional", "legacy" }),
                new FallbackVendorManifestRequiredNameRule("fallback vendor manifest needs this field"),
                new FallbackVendorManifestOwnerIdLengthRule(64),
                new FallbackVendorManifestNameLengthRule(3600),
            });
    }
}
