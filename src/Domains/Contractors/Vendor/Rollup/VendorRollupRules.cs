namespace Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>A rule a vendor rollup must pass. Check returns its problems, or none.</summary>
public interface IVendorRollupRule
{
    List<string> Check(VendorRollup item);
}

public sealed record VendorRollupRequiredDepartmentRule(string Message) : IVendorRollupRule
{
    public List<string> Check(VendorRollup item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorRollupCodeLengthRule(int MaxLength) : IVendorRollupRule
{
    public List<string> Check(VendorRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorRollupRequiredTitleRule(string Message) : IVendorRollupRule
{
    public List<string> Check(VendorRollup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor rollup.</summary>
public sealed record VendorRollupValidator(List<IVendorRollupRule> Rules)
{
    public List<string> Validate(VendorRollup item)
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

    public void RequireValid(VendorRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorRollup> items)
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

/// <summary>Functions of vendor rollup rules.</summary>
public static class VendorRollupRules
{
    public static VendorRollupValidator DefaultVendorRollupValidator()
    {
        return new VendorRollupValidator(
            new List<IVendorRollupRule>
            {
                new VendorRollupRequiredDepartmentRule("vendor rollup needs this field"),
                new VendorRollupCodeLengthRule(255),
                new VendorRollupRequiredTitleRule("vendor rollup needs a value"),
            });
    }
}
