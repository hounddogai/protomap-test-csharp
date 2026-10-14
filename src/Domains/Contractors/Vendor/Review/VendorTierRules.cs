namespace Acme.Workforce.Contractors.Vendor.Review;

/// <summary>A rule a vendor tier must pass. Check returns its problems, or none.</summary>
public interface IVendorTierRule
{
    List<string> Check(VendorTier item);
}

public sealed record VendorTierCostRangeRule(double Min, double Max) : IVendorTierRule
{
    public List<string> Check(VendorTier item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorTierRequiredNameRule(string Message) : IVendorTierRule
{
    public List<string> Check(VendorTier item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorTierRequiredCountryRule(string Message) : IVendorTierRule
{
    public List<string> Check(VendorTier item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorTierRequiredCodeRule(string Message) : IVendorTierRule
{
    public List<string> Check(VendorTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor tier.</summary>
public sealed record VendorTierValidator(List<IVendorTierRule> Rules)
{
    public List<string> Validate(VendorTier item)
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

    public void RequireValid(VendorTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorTier> items)
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

/// <summary>Functions of vendor tier rules.</summary>
public static class VendorTierRules
{
    public static VendorTierValidator DefaultVendorTierValidator()
    {
        return new VendorTierValidator(
            new List<IVendorTierRule>
            {
                new VendorTierCostRangeRule(1.5, 1344.4),
                new VendorTierRequiredNameRule("vendor tier needs an entry"),
                new VendorTierRequiredCountryRule("vendor tier needs an entry"),
                new VendorTierRequiredCodeRule("vendor tier needs a value"),
            });
    }
}
