namespace Acme.Workforce.Assets.Warranty.Quota;

/// <summary>A rule a warranty quota must pass. Check returns its problems, or none.</summary>
public interface IWarrantyQuotaRule
{
    List<string> Check(WarrantyQuota item);
}

public sealed record WarrantyQuotaNameLengthRule(int MaxLength) : IWarrantyQuotaRule
{
    public List<string> Check(WarrantyQuota item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyQuotaCodePrefixRule(string Prefix) : IWarrantyQuotaRule
{
    public List<string> Check(WarrantyQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyQuotaTagsLimitRule(int MaxCount, List<string> Banned) : IWarrantyQuotaRule
{
    public List<string> Check(WarrantyQuota item)
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

/// <summary>Runs every rule of a warranty quota.</summary>
public sealed record WarrantyQuotaValidator(List<IWarrantyQuotaRule> Rules)
{
    public List<string> Validate(WarrantyQuota item)
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

    public void RequireValid(WarrantyQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyQuota> items)
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

/// <summary>Functions of warranty quota rules.</summary>
public static class WarrantyQuotaRules
{
    public static WarrantyQuotaValidator DefaultWarrantyQuotaValidator()
    {
        return new WarrantyQuotaValidator(
            new List<IWarrantyQuotaRule>
            {
                new WarrantyQuotaNameLengthRule(64),
                new WarrantyQuotaCodePrefixRule("Q"),
                new WarrantyQuotaTagsLimitRule(120, new List<string>()),
            });
    }
}
