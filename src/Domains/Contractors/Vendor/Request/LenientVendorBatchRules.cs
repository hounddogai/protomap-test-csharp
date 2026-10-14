namespace Acme.Workforce.Contractors.Vendor.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient vendor batch must pass. Check returns its problems, or none.</summary>
public interface ILenientVendorBatchRule
{
    List<string> Check(LenientVendorBatch item);
}

public sealed record LenientVendorBatchRequiredLabelRule(string Message) : ILenientVendorBatchRule
{
    public List<string> Check(LenientVendorBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientVendorBatchAllowedPhoneRule(List<string> Allowed) : ILenientVendorBatchRule
{
    public List<string> Check(LenientVendorBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record LenientVendorBatchFloorValueRangeRule(double Min, double Max) : ILenientVendorBatchRule
{
    public List<string> Check(LenientVendorBatch item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVendorBatchLabelLengthRule(int MaxLength) : ILenientVendorBatchRule
{
    public List<string> Check(LenientVendorBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient vendor batch.</summary>
public sealed record LenientVendorBatchValidator(List<ILenientVendorBatchRule> Rules)
{
    public List<string> Validate(LenientVendorBatch item)
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

    public void RequireValid(LenientVendorBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientVendorBatch> items)
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

/// <summary>Functions of lenient vendor batch rules.</summary>
public static class LenientVendorBatchRules
{
    public static LenientVendorBatchValidator DefaultLenientVendorBatchValidator()
    {
        return new LenientVendorBatchValidator(
            new List<ILenientVendorBatchRule>
            {
                new LenientVendorBatchRequiredLabelRule("lenient vendor batch needs a value"),
                new LenientVendorBatchAllowedPhoneRule(new List<string> { "rolling", "scheduled" }),
                new LenientVendorBatchFloorValueRangeRule(7.6, 2733.7),
                new LenientVendorBatchLabelLengthRule(80),
            });
    }
}
