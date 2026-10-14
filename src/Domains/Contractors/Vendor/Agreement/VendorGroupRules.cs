namespace Acme.Workforce.Contractors.Vendor.Agreement;

/// <summary>A rule a vendor group must pass. Check returns its problems, or none.</summary>
public interface IVendorGroupRule
{
    List<string> Check(VendorGroup item);
}

public sealed record VendorGroupManagerIdLengthRule(int MaxLength) : IVendorGroupRule
{
    public List<string> Check(VendorGroup item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorGroupRatioRangeRule(double Min, double Max) : IVendorGroupRule
{
    public List<string> Check(VendorGroup item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorGroupRequiredCodeRule(string Message) : IVendorGroupRule
{
    public List<string> Check(VendorGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorGroupWindowDaysRangeRule(int Min, int Max) : IVendorGroupRule
{
    public List<string> Check(VendorGroup item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorGroupRequiredManagerIdRule(string Message) : IVendorGroupRule
{
    public List<string> Check(VendorGroup item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor group.</summary>
public sealed record VendorGroupValidator(List<IVendorGroupRule> Rules)
{
    public List<string> Validate(VendorGroup item)
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

    public void RequireValid(VendorGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorGroup> items)
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

/// <summary>Functions of vendor group rules.</summary>
public static class VendorGroupRules
{
    public static VendorGroupValidator DefaultVendorGroupValidator()
    {
        return new VendorGroupValidator(
            new List<IVendorGroupRule>
            {
                new VendorGroupManagerIdLengthRule(120),
                new VendorGroupRatioRangeRule(0.7, 4171.4),
                new VendorGroupRequiredCodeRule("vendor group needs an entry"),
                new VendorGroupWindowDaysRangeRule(32, 86400),
                new VendorGroupRequiredManagerIdRule("vendor group needs this field"),
            });
    }
}
