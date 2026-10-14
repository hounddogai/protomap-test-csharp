namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a vendor request must pass. Check returns its problems, or none.</summary>
public interface IVendorRequestRule
{
    List<string> Check(VendorRequest item);
}

public sealed record VendorRequestDepartmentLengthRule(int MaxLength) : IVendorRequestRule
{
    public List<string> Check(VendorRequest item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorRequestAllowedLabelRule(List<string> Allowed) : IVendorRequestRule
{
    public List<string> Check(VendorRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

public sealed record VendorRequestCodeLengthRule(int MaxLength) : IVendorRequestRule
{
    public List<string> Check(VendorRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor request.</summary>
public sealed record VendorRequestValidator(List<IVendorRequestRule> Rules)
{
    public List<string> Validate(VendorRequest item)
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

    public void RequireValid(VendorRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorRequest> items)
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

/// <summary>Functions of vendor request rules.</summary>
public static class VendorRequestRules
{
    public static VendorRequestValidator DefaultVendorRequestValidator()
    {
        return new VendorRequestValidator(
            new List<IVendorRequestRule>
            {
                new VendorRequestDepartmentLengthRule(86400),
                new VendorRequestAllowedLabelRule(new List<string> { "default", "weighted" }),
                new VendorRequestCodeLengthRule(64),
            });
    }
}
