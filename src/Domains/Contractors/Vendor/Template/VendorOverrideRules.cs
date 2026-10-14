namespace Acme.Workforce.Contractors.Vendor.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a vendor override must pass. Check returns its problems, or none.</summary>
public interface IVendorOverrideRule
{
    List<string> Check(VendorOverride item);
}

public sealed record VendorOverrideCodeLengthRule(int MaxLength) : IVendorOverrideRule
{
    public List<string> Check(VendorOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorOverridePhoneLengthRule(int MaxLength) : IVendorOverrideRule
{
    public List<string> Check(VendorOverride item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorOverrideAllowedCodeRule(List<string> Allowed) : IVendorOverrideRule
{
    public List<string> Check(VendorOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record VendorOverrideCreatedAtRangeRule(int Min, int Max) : IVendorOverrideRule
{
    public List<string> Check(VendorOverride item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorOverrideCodePrefixRule(string Prefix) : IVendorOverrideRule
{
    public List<string> Check(VendorOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VendorOverrideRequiredLabelRule(string Message) : IVendorOverrideRule
{
    public List<string> Check(VendorOverride item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor override.</summary>
public sealed record VendorOverrideValidator(List<IVendorOverrideRule> Rules)
{
    public List<string> Validate(VendorOverride item)
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

    public void RequireValid(VendorOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorOverride> items)
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

/// <summary>Functions of vendor override rules.</summary>
public static class VendorOverrideRules
{
    public static VendorOverrideValidator DefaultVendorOverrideValidator()
    {
        return new VendorOverrideValidator(
            new List<IVendorOverrideRule>
            {
                new VendorOverrideCodeLengthRule(32),
                new VendorOverridePhoneLengthRule(16),
                new VendorOverrideAllowedCodeRule(new List<string> { "manual", "scheduled", "strict" }),
                new VendorOverrideCreatedAtRangeRule(255, 255),
                new VendorOverrideCodePrefixRule("OPS"),
                new VendorOverrideRequiredLabelRule("vendor override needs this field"),
            });
    }
}
