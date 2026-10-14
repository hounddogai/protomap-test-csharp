namespace Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>A rule a vendor assignment must pass. Check returns its problems, or none.</summary>
public interface IVendorAssignmentRule
{
    List<string> Check(VendorAssignment item);
}

public sealed record VendorAssignmentCapacityRangeRule(int Min, int Max) : IVendorAssignmentRule
{
    public List<string> Check(VendorAssignment item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorAssignmentCodeLengthRule(int MaxLength) : IVendorAssignmentRule
{
    public List<string> Check(VendorAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorAssignmentTagsLimitRule(int MaxCount, List<string> Banned) : IVendorAssignmentRule
{
    public List<string> Check(VendorAssignment item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record VendorAssignmentOwnerIdLengthRule(int MaxLength) : IVendorAssignmentRule
{
    public List<string> Check(VendorAssignment item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor assignment.</summary>
public sealed record VendorAssignmentValidator(List<IVendorAssignmentRule> Rules)
{
    public List<string> Validate(VendorAssignment item)
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

    public void RequireValid(VendorAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorAssignment> items)
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

/// <summary>Functions of vendor assignment rules.</summary>
public static class VendorAssignmentRules
{
    public static VendorAssignmentValidator DefaultVendorAssignmentValidator()
    {
        return new VendorAssignmentValidator(
            new List<IVendorAssignmentRule>
            {
                new VendorAssignmentCapacityRangeRule(120, 3600),
                new VendorAssignmentCodeLengthRule(120),
                new VendorAssignmentTagsLimitRule(80, new List<string> { "regional", "legacy" }),
                new VendorAssignmentOwnerIdLengthRule(32),
            });
    }
}
