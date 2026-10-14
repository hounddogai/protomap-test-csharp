namespace Acme.Workforce.Contractors.Vendor.Review;

/// <summary>A rule a vendor manifest must pass. Check returns its problems, or none.</summary>
public interface IVendorManifestRule
{
    List<string> Check(VendorManifest item);
}

public sealed record VendorManifestLabelLengthRule(int MaxLength) : IVendorManifestRule
{
    public List<string> Check(VendorManifest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorManifestThresholdRangeRule(double Min, double Max) : IVendorManifestRule
{
    public List<string> Check(VendorManifest item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorManifestMarginRangeRule(double Min, double Max) : IVendorManifestRule
{
    public List<string> Check(VendorManifest item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorManifestCodeLengthRule(int MaxLength) : IVendorManifestRule
{
    public List<string> Check(VendorManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor manifest.</summary>
public sealed record VendorManifestValidator(List<IVendorManifestRule> Rules)
{
    public List<string> Validate(VendorManifest item)
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

    public void RequireValid(VendorManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorManifest> items)
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

/// <summary>Functions of vendor manifest rules.</summary>
public static class VendorManifestRules
{
    public static VendorManifestValidator DefaultVendorManifestValidator()
    {
        return new VendorManifestValidator(
            new List<IVendorManifestRule>
            {
                new VendorManifestLabelLengthRule(86400),
                new VendorManifestThresholdRangeRule(4.1, 4320.4),
                new VendorManifestMarginRangeRule(0.4, 1448.0),
                new VendorManifestCodeLengthRule(80),
            });
    }
}
