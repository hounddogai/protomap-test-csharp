namespace Acme.Workforce.Contractors.Vendor.Variance;

/// <summary>A rule a vendor adjustment must pass. Check returns its problems, or none.</summary>
public interface IVendorAdjustmentRule
{
    List<string> Check(VendorAdjustment item);
}

public sealed record VendorAdjustmentCodeLengthRule(int MaxLength) : IVendorAdjustmentRule
{
    public List<string> Check(VendorAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorAdjustmentRequiredTeamRule(string Message) : IVendorAdjustmentRule
{
    public List<string> Check(VendorAdjustment item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorAdjustmentCodePrefixRule(string Prefix) : IVendorAdjustmentRule
{
    public List<string> Check(VendorAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor adjustment.</summary>
public sealed record VendorAdjustmentValidator(List<IVendorAdjustmentRule> Rules)
{
    public List<string> Validate(VendorAdjustment item)
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

    public void RequireValid(VendorAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorAdjustment> items)
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

/// <summary>Functions of vendor adjustment rules.</summary>
public static class VendorAdjustmentRules
{
    public static VendorAdjustmentValidator DefaultVendorAdjustmentValidator()
    {
        return new VendorAdjustmentValidator(
            new List<IVendorAdjustmentRule>
            {
                new VendorAdjustmentCodeLengthRule(3600),
                new VendorAdjustmentRequiredTeamRule("vendor adjustment needs a value"),
                new VendorAdjustmentCodePrefixRule("X"),
            });
    }
}
