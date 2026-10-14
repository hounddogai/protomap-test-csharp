namespace Acme.Workforce.Contractors.Vendor.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a vendor batch must pass. Check returns its problems, or none.</summary>
public interface IVendorBatchRule
{
    List<string> Check(VendorBatch item);
}

public sealed record VendorBatchCodeLengthRule(int MaxLength) : IVendorBatchRule
{
    public List<string> Check(VendorBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorBatchRequiredPhoneRule(string Message) : IVendorBatchRule
{
    public List<string> Check(VendorBatch item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorBatchRequiredLabelRule(string Message) : IVendorBatchRule
{
    public List<string> Check(VendorBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorBatchAllowedSummaryRule(List<string> Allowed) : IVendorBatchRule
{
    public List<string> Check(VendorBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record VendorBatchLabelLengthRule(int MaxLength) : IVendorBatchRule
{
    public List<string> Check(VendorBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor batch.</summary>
public sealed record VendorBatchValidator(List<IVendorBatchRule> Rules)
{
    public List<string> Validate(VendorBatch item)
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

    public void RequireValid(VendorBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorBatch> items)
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

/// <summary>Functions of vendor batch rules.</summary>
public static class VendorBatchRules
{
    public static VendorBatchValidator DefaultVendorBatchValidator()
    {
        return new VendorBatchValidator(
            new List<IVendorBatchRule>
            {
                new VendorBatchCodeLengthRule(80),
                new VendorBatchRequiredPhoneRule("vendor batch needs a value"),
                new VendorBatchRequiredLabelRule("vendor batch needs this field"),
                new VendorBatchAllowedSummaryRule(new List<string> { "default" }),
                new VendorBatchLabelLengthRule(80),
            });
    }
}
