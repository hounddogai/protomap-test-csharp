namespace Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>A rule a vendor allocation must pass. Check returns its problems, or none.</summary>
public interface IVendorAllocationRule
{
    List<string> Check(VendorAllocation item);
}

public sealed record VendorAllocationEndsAtRangeRule(int Min, int Max) : IVendorAllocationRule
{
    public List<string> Check(VendorAllocation item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorAllocationCodePrefixRule(string Prefix) : IVendorAllocationRule
{
    public List<string> Check(VendorAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VendorAllocationSkillLengthRule(int MaxLength) : IVendorAllocationRule
{
    public List<string> Check(VendorAllocation item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorAllocationRequiredCodeRule(string Message) : IVendorAllocationRule
{
    public List<string> Check(VendorAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor allocation.</summary>
public sealed record VendorAllocationValidator(List<IVendorAllocationRule> Rules)
{
    public List<string> Validate(VendorAllocation item)
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

    public void RequireValid(VendorAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorAllocation> items)
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

/// <summary>Functions of vendor allocation rules.</summary>
public static class VendorAllocationRules
{
    public static VendorAllocationValidator DefaultVendorAllocationValidator()
    {
        return new VendorAllocationValidator(
            new List<IVendorAllocationRule>
            {
                new VendorAllocationEndsAtRangeRule(64, 3600),
                new VendorAllocationCodePrefixRule("Q"),
                new VendorAllocationSkillLengthRule(86400),
                new VendorAllocationRequiredCodeRule("vendor allocation needs this field"),
            });
    }
}
