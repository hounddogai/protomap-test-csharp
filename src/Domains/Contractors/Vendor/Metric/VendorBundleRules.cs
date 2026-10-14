namespace Acme.Workforce.Contractors.Vendor.Metric;

/// <summary>A rule a vendor bundle must pass. Check returns its problems, or none.</summary>
public interface IVendorBundleRule
{
    List<string> Check(VendorBundle item);
}

public sealed record VendorBundleRatingRangeRule(double Min, double Max) : IVendorBundleRule
{
    public List<string> Check(VendorBundle item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorBundleLeaveDaysRangeRule(int Min, int Max) : IVendorBundleRule
{
    public List<string> Check(VendorBundle item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorBundleRequiredSkillRule(string Message) : IVendorBundleRule
{
    public List<string> Check(VendorBundle item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorBundleSkillLengthRule(int MaxLength) : IVendorBundleRule
{
    public List<string> Check(VendorBundle item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor bundle.</summary>
public sealed record VendorBundleValidator(List<IVendorBundleRule> Rules)
{
    public List<string> Validate(VendorBundle item)
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

    public void RequireValid(VendorBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorBundle> items)
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

/// <summary>Functions of vendor bundle rules.</summary>
public static class VendorBundleRules
{
    public static VendorBundleValidator DefaultVendorBundleValidator()
    {
        return new VendorBundleValidator(
            new List<IVendorBundleRule>
            {
                new VendorBundleRatingRangeRule(2.7, 2300.5),
                new VendorBundleLeaveDaysRangeRule(120, 1000),
                new VendorBundleRequiredSkillRule("vendor bundle needs this field"),
                new VendorBundleSkillLengthRule(80),
            });
    }
}
