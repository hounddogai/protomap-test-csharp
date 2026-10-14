namespace Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>A rule a vendor slot must pass. Check returns its problems, or none.</summary>
public interface IVendorSlotRule
{
    List<string> Check(VendorSlot item);
}

public sealed record VendorSlotRequiredTitleRule(string Message) : IVendorSlotRule
{
    public List<string> Check(VendorSlot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorSlotCodePrefixRule(string Prefix) : IVendorSlotRule
{
    public List<string> Check(VendorSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VendorSlotCodeLengthRule(int MaxLength) : IVendorSlotRule
{
    public List<string> Check(VendorSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorSlotLocaleLengthRule(int MaxLength) : IVendorSlotRule
{
    public List<string> Check(VendorSlot item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorSlotLocationLengthRule(int MaxLength) : IVendorSlotRule
{
    public List<string> Check(VendorSlot item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorSlotRequiredLocationRule(string Message) : IVendorSlotRule
{
    public List<string> Check(VendorSlot item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor slot.</summary>
public sealed record VendorSlotValidator(List<IVendorSlotRule> Rules)
{
    public List<string> Validate(VendorSlot item)
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

    public void RequireValid(VendorSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorSlot> items)
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

/// <summary>Functions of vendor slot rules.</summary>
public static class VendorSlotRules
{
    public static VendorSlotValidator DefaultVendorSlotValidator()
    {
        return new VendorSlotValidator(
            new List<IVendorSlotRule>
            {
                new VendorSlotRequiredTitleRule("vendor slot needs a value"),
                new VendorSlotCodePrefixRule("REF"),
                new VendorSlotCodeLengthRule(16),
                new VendorSlotLocaleLengthRule(32),
                new VendorSlotLocationLengthRule(64),
                new VendorSlotRequiredLocationRule("vendor slot needs this field"),
            });
    }
}
