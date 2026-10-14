namespace Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>A rule a vendor policy must pass. Check returns its problems, or none.</summary>
public interface IVendorPolicyRule
{
    List<string> Check(VendorPolicy item);
}

public sealed record VendorPolicyTitleLengthRule(int MaxLength) : IVendorPolicyRule
{
    public List<string> Check(VendorPolicy item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorPolicyVersionRangeRule(int Min, int Max) : IVendorPolicyRule
{
    public List<string> Check(VendorPolicy item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorPolicyWeightRangeRule(double Min, double Max) : IVendorPolicyRule
{
    public List<string> Check(VendorPolicy item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorPolicyRequiredTitleRule(string Message) : IVendorPolicyRule
{
    public List<string> Check(VendorPolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorPolicyCountryLengthRule(int MaxLength) : IVendorPolicyRule
{
    public List<string> Check(VendorPolicy item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor policy.</summary>
public sealed record VendorPolicyValidator(List<IVendorPolicyRule> Rules)
{
    public List<string> Validate(VendorPolicy item)
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

    public void RequireValid(VendorPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorPolicy> items)
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

/// <summary>Functions of vendor policy rules.</summary>
public static class VendorPolicyRules
{
    public static VendorPolicyValidator DefaultVendorPolicyValidator()
    {
        return new VendorPolicyValidator(
            new List<IVendorPolicyRule>
            {
                new VendorPolicyTitleLengthRule(3600),
                new VendorPolicyVersionRangeRule(80, 120),
                new VendorPolicyWeightRangeRule(0.3, 3322.5),
                new VendorPolicyRequiredTitleRule("vendor policy needs an entry"),
                new VendorPolicyCountryLengthRule(16),
            });
    }
}
