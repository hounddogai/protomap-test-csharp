namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>A rule a vendor threshold must pass. Check returns its problems, or none.</summary>
public interface IVendorThresholdRule
{
    List<string> Check(VendorThreshold item);
}

public sealed record VendorThresholdNameLengthRule(int MaxLength) : IVendorThresholdRule
{
    public List<string> Check(VendorThreshold item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorThresholdRequiredCodeRule(string Message) : IVendorThresholdRule
{
    public List<string> Check(VendorThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorThresholdCodePrefixRule(string Prefix) : IVendorThresholdRule
{
    public List<string> Check(VendorThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VendorThresholdHoursRangeRule(double Min, double Max) : IVendorThresholdRule
{
    public List<string> Check(VendorThreshold item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorThresholdCodeLengthRule(int MaxLength) : IVendorThresholdRule
{
    public List<string> Check(VendorThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor threshold.</summary>
public sealed record VendorThresholdValidator(List<IVendorThresholdRule> Rules)
{
    public List<string> Validate(VendorThreshold item)
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

    public void RequireValid(VendorThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorThreshold> items)
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

/// <summary>Functions of vendor threshold rules.</summary>
public static class VendorThresholdRules
{
    public static VendorThresholdValidator DefaultVendorThresholdValidator()
    {
        return new VendorThresholdValidator(
            new List<IVendorThresholdRule>
            {
                new VendorThresholdNameLengthRule(86400),
                new VendorThresholdRequiredCodeRule("vendor threshold needs a value"),
                new VendorThresholdCodePrefixRule("OPS"),
                new VendorThresholdHoursRangeRule(2.6, 1714.9),
                new VendorThresholdCodeLengthRule(16),
            });
    }
}
