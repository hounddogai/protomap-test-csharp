namespace Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a vendor profile must pass. Check returns its problems, or none.</summary>
public interface IVendorProfileRule
{
    List<string> Check(VendorProfile item);
}

public sealed record VendorProfileRequiredLabelRule(string Message) : IVendorProfileRule
{
    public List<string> Check(VendorProfile item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorProfileLabelLengthRule(int MaxLength) : IVendorProfileRule
{
    public List<string> Check(VendorProfile item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorProfileMaximumRangeRule(double Min, double Max) : IVendorProfileRule
{
    public List<string> Check(VendorProfile item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorProfileRegionLengthRule(int MaxLength) : IVendorProfileRule
{
    public List<string> Check(VendorProfile item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorProfileAllowedLabelRule(List<string> Allowed) : IVendorProfileRule
{
    public List<string> Check(VendorProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor profile.</summary>
public sealed record VendorProfileValidator(List<IVendorProfileRule> Rules)
{
    public List<string> Validate(VendorProfile item)
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

    public void RequireValid(VendorProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorProfile> items)
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

/// <summary>Functions of vendor profile rules.</summary>
public static class VendorProfileRules
{
    public static VendorProfileValidator DefaultVendorProfileValidator()
    {
        return new VendorProfileValidator(
            new List<IVendorProfileRule>
            {
                new VendorProfileRequiredLabelRule("vendor profile needs an entry"),
                new VendorProfileLabelLengthRule(80),
                new VendorProfileMaximumRangeRule(4.5, 4803.1),
                new VendorProfileRegionLengthRule(120),
                new VendorProfileAllowedLabelRule(new List<string> { "strict" }),
            });
    }
}
