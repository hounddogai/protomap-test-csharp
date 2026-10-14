namespace Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>A rule a pending vendor manifest must pass. Check returns its problems, or none.</summary>
public interface IPendingVendorManifestRule
{
    List<string> Check(PendingVendorManifest item);
}

public sealed record PendingVendorManifestRequiredPhoneRule(string Message) : IPendingVendorManifestRule
{
    public List<string> Check(PendingVendorManifest item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingVendorManifestDiscountRangeRule(double Min, double Max) : IPendingVendorManifestRule
{
    public List<string> Check(PendingVendorManifest item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingVendorManifestRegionLengthRule(int MaxLength) : IPendingVendorManifestRule
{
    public List<string> Check(PendingVendorManifest item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingVendorManifestRequiredCodeRule(string Message) : IPendingVendorManifestRule
{
    public List<string> Check(PendingVendorManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingVendorManifestRequiredRegionRule(string Message) : IPendingVendorManifestRule
{
    public List<string> Check(PendingVendorManifest item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending vendor manifest.</summary>
public sealed record PendingVendorManifestValidator(List<IPendingVendorManifestRule> Rules)
{
    public List<string> Validate(PendingVendorManifest item)
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

    public void RequireValid(PendingVendorManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingVendorManifest> items)
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

/// <summary>Functions of pending vendor manifest rules.</summary>
public static class PendingVendorManifestRules
{
    public static PendingVendorManifestValidator DefaultPendingVendorManifestValidator()
    {
        return new PendingVendorManifestValidator(
            new List<IPendingVendorManifestRule>
            {
                new PendingVendorManifestRequiredPhoneRule("pending vendor manifest needs an entry"),
                new PendingVendorManifestDiscountRangeRule(3.3, 1377.3),
                new PendingVendorManifestRegionLengthRule(64),
                new PendingVendorManifestRequiredCodeRule("pending vendor manifest needs an entry"),
                new PendingVendorManifestRequiredRegionRule("pending vendor manifest needs a value"),
            });
    }
}
