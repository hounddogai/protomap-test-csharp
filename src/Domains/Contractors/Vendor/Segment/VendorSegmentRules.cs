namespace Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>A rule a vendor segment must pass. Check returns its problems, or none.</summary>
public interface IVendorSegmentRule
{
    List<string> Check(VendorSegment item);
}

public sealed record VendorSegmentCodePrefixRule(string Prefix) : IVendorSegmentRule
{
    public List<string> Check(VendorSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VendorSegmentRequiredCurrencyRule(string Message) : IVendorSegmentRule
{
    public List<string> Check(VendorSegment item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorSegmentOwnerIdLengthRule(int MaxLength) : IVendorSegmentRule
{
    public List<string> Check(VendorSegment item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorSegmentLeaveDaysRangeRule(int Min, int Max) : IVendorSegmentRule
{
    public List<string> Check(VendorSegment item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorSegmentCurrencyLengthRule(int MaxLength) : IVendorSegmentRule
{
    public List<string> Check(VendorSegment item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor segment.</summary>
public sealed record VendorSegmentValidator(List<IVendorSegmentRule> Rules)
{
    public List<string> Validate(VendorSegment item)
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

    public void RequireValid(VendorSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorSegment> items)
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

/// <summary>Functions of vendor segment rules.</summary>
public static class VendorSegmentRules
{
    public static VendorSegmentValidator DefaultVendorSegmentValidator()
    {
        return new VendorSegmentValidator(
            new List<IVendorSegmentRule>
            {
                new VendorSegmentCodePrefixRule("REF"),
                new VendorSegmentRequiredCurrencyRule("vendor segment needs an entry"),
                new VendorSegmentOwnerIdLengthRule(120),
                new VendorSegmentLeaveDaysRangeRule(64, 255),
                new VendorSegmentCurrencyLengthRule(3600),
            });
    }
}
