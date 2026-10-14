namespace Acme.Workforce.Assets.Warranty.Summary;

/// <summary>A rule a warranty bundle must pass. Check returns its problems, or none.</summary>
public interface IWarrantyBundleRule
{
    List<string> Check(WarrantyBundle item);
}

public sealed record WarrantyBundleRequiredBadgeRule(string Message) : IWarrantyBundleRule
{
    public List<string> Check(WarrantyBundle item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBundleCodePrefixRule(string Prefix) : IWarrantyBundleRule
{
    public List<string> Check(WarrantyBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBundleRequiredCodeRule(string Message) : IWarrantyBundleRule
{
    public List<string> Check(WarrantyBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty bundle.</summary>
public sealed record WarrantyBundleValidator(List<IWarrantyBundleRule> Rules)
{
    public List<string> Validate(WarrantyBundle item)
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

    public void RequireValid(WarrantyBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyBundle> items)
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

/// <summary>Functions of warranty bundle rules.</summary>
public static class WarrantyBundleRules
{
    public static WarrantyBundleValidator DefaultWarrantyBundleValidator()
    {
        return new WarrantyBundleValidator(
            new List<IWarrantyBundleRule>
            {
                new WarrantyBundleRequiredBadgeRule("warranty bundle needs this field"),
                new WarrantyBundleCodePrefixRule("OPS"),
                new WarrantyBundleRequiredCodeRule("warranty bundle needs a value"),
            });
    }
}
