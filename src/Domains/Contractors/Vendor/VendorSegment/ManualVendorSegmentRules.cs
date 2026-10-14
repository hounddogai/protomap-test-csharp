namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>A rule a manual vendor segment must pass. Check returns its problems, or none.</summary>
public interface IManualVendorSegmentRule
{
    List<string> Check(ManualVendorSegment item);
}

public sealed record ManualVendorSegmentCodePrefixRule(string Prefix) : IManualVendorSegmentRule
{
    public List<string> Check(ManualVendorSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualVendorSegmentTitleLengthRule(int MaxLength) : IManualVendorSegmentRule
{
    public List<string> Check(ManualVendorSegment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualVendorSegmentMaximumRangeRule(double Min, double Max) : IManualVendorSegmentRule
{
    public List<string> Check(ManualVendorSegment item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual vendor segment.</summary>
public sealed record ManualVendorSegmentValidator(List<IManualVendorSegmentRule> Rules)
{
    public List<string> Validate(ManualVendorSegment item)
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

    public void RequireValid(ManualVendorSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualVendorSegment> items)
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

/// <summary>Functions of manual vendor segment rules.</summary>
public static class ManualVendorSegmentRules
{
    public static ManualVendorSegmentValidator DefaultManualVendorSegmentValidator()
    {
        return new ManualVendorSegmentValidator(
            new List<IManualVendorSegmentRule>
            {
                new ManualVendorSegmentCodePrefixRule("X"),
                new ManualVendorSegmentTitleLengthRule(80),
                new ManualVendorSegmentMaximumRangeRule(0.9, 2205.3),
            });
    }
}
