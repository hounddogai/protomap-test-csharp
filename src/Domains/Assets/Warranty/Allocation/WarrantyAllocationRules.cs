namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a warranty allocation must pass. Check returns its problems, or none.</summary>
public interface IWarrantyAllocationRule
{
    List<string> Check(WarrantyAllocation item);
}

public sealed record WarrantyAllocationAllowedSegmentRule(List<string> Allowed) : IWarrantyAllocationRule
{
    public List<string> Check(WarrantyAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is expired" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyAllocationRequiredSegmentRule(string Message) : IWarrantyAllocationRule
{
    public List<string> Check(WarrantyAllocation item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyAllocationRequiredNameRule(string Message) : IWarrantyAllocationRule
{
    public List<string> Check(WarrantyAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty allocation.</summary>
public sealed record WarrantyAllocationValidator(List<IWarrantyAllocationRule> Rules)
{
    public List<string> Validate(WarrantyAllocation item)
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

    public void RequireValid(WarrantyAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyAllocation> items)
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

/// <summary>Functions of warranty allocation rules.</summary>
public static class WarrantyAllocationRules
{
    public static WarrantyAllocationValidator DefaultWarrantyAllocationValidator()
    {
        return new WarrantyAllocationValidator(
            new List<IWarrantyAllocationRule>
            {
                new WarrantyAllocationAllowedSegmentRule(new List<string> { "seasonal" }),
                new WarrantyAllocationRequiredSegmentRule("warranty allocation needs a value"),
                new WarrantyAllocationRequiredNameRule("warranty allocation needs this field"),
            });
    }
}
