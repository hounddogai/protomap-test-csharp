namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a vendor milestone must pass. Check returns its problems, or none.</summary>
public interface IVendorMilestoneRule
{
    List<string> Check(VendorMilestone item);
}

public sealed record VendorMilestoneQuantityRangeRule(int Min, int Max) : IVendorMilestoneRule
{
    public List<string> Check(VendorMilestone item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorMilestoneRegionLengthRule(int MaxLength) : IVendorMilestoneRule
{
    public List<string> Check(VendorMilestone item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorMilestoneUpdatedAtRangeRule(int Min, int Max) : IVendorMilestoneRule
{
    public List<string> Check(VendorMilestone item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorMilestoneAllowedBadgeRule(List<string> Allowed) : IVendorMilestoneRule
{
    public List<string> Check(VendorMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor milestone.</summary>
public sealed record VendorMilestoneValidator(List<IVendorMilestoneRule> Rules)
{
    public List<string> Validate(VendorMilestone item)
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

    public void RequireValid(VendorMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorMilestone> items)
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

/// <summary>Functions of vendor milestone rules.</summary>
public static class VendorMilestoneRules
{
    public static VendorMilestoneValidator DefaultVendorMilestoneValidator()
    {
        return new VendorMilestoneValidator(
            new List<IVendorMilestoneRule>
            {
                new VendorMilestoneQuantityRangeRule(16, 64),
                new VendorMilestoneRegionLengthRule(3600),
                new VendorMilestoneUpdatedAtRangeRule(255, 255),
                new VendorMilestoneAllowedBadgeRule(new List<string> { "lenient" }),
            });
    }
}
