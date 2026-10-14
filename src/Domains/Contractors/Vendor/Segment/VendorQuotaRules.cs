namespace Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>A rule a vendor quota must pass. Check returns its problems, or none.</summary>
public interface IVendorQuotaRule
{
    List<string> Check(VendorQuota item);
}

public sealed record VendorQuotaTitleLengthRule(int MaxLength) : IVendorQuotaRule
{
    public List<string> Check(VendorQuota item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorQuotaSegmentLengthRule(int MaxLength) : IVendorQuotaRule
{
    public List<string> Check(VendorQuota item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorQuotaRequiredSegmentRule(string Message) : IVendorQuotaRule
{
    public List<string> Check(VendorQuota item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor quota.</summary>
public sealed record VendorQuotaValidator(List<IVendorQuotaRule> Rules)
{
    public List<string> Validate(VendorQuota item)
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

    public void RequireValid(VendorQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorQuota> items)
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

/// <summary>Functions of vendor quota rules.</summary>
public static class VendorQuotaRules
{
    public static VendorQuotaValidator DefaultVendorQuotaValidator()
    {
        return new VendorQuotaValidator(
            new List<IVendorQuotaRule>
            {
                new VendorQuotaTitleLengthRule(120),
                new VendorQuotaSegmentLengthRule(3600),
                new VendorQuotaRequiredSegmentRule("vendor quota needs a value"),
            });
    }
}
