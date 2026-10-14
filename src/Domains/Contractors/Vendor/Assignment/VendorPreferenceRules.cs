namespace Acme.Workforce.Contractors.Vendor.Assignment;

/// <summary>A rule a vendor preference must pass. Check returns its problems, or none.</summary>
public interface IVendorPreferenceRule
{
    List<string> Check(VendorPreference item);
}

public sealed record VendorPreferenceRequiredRegionRule(string Message) : IVendorPreferenceRule
{
    public List<string> Check(VendorPreference item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorPreferenceCodePrefixRule(string Prefix) : IVendorPreferenceRule
{
    public List<string> Check(VendorPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VendorPreferenceCodeLengthRule(int MaxLength) : IVendorPreferenceRule
{
    public List<string> Check(VendorPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorPreferenceAmountRangeRule(double Min, double Max) : IVendorPreferenceRule
{
    public List<string> Check(VendorPreference item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor preference.</summary>
public sealed record VendorPreferenceValidator(List<IVendorPreferenceRule> Rules)
{
    public List<string> Validate(VendorPreference item)
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

    public void RequireValid(VendorPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorPreference> items)
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

/// <summary>Functions of vendor preference rules.</summary>
public static class VendorPreferenceRules
{
    public static VendorPreferenceValidator DefaultVendorPreferenceValidator()
    {
        return new VendorPreferenceValidator(
            new List<IVendorPreferenceRule>
            {
                new VendorPreferenceRequiredRegionRule("vendor preference needs this field"),
                new VendorPreferenceCodePrefixRule("Q"),
                new VendorPreferenceCodeLengthRule(16),
                new VendorPreferenceAmountRangeRule(5.8, 4666.3),
            });
    }
}
