namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>A rule a vendor window must pass. Check returns its problems, or none.</summary>
public interface IVendorWindowRule
{
    List<string> Check(VendorWindow item);
}

public sealed record VendorWindowNameLengthRule(int MaxLength) : IVendorWindowRule
{
    public List<string> Check(VendorWindow item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorWindowRequiredNameRule(string Message) : IVendorWindowRule
{
    public List<string> Check(VendorWindow item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorWindowFloorValueRangeRule(double Min, double Max) : IVendorWindowRule
{
    public List<string> Check(VendorWindow item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorWindowMinimumRangeRule(double Min, double Max) : IVendorWindowRule
{
    public List<string> Check(VendorWindow item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorWindowCountryLengthRule(int MaxLength) : IVendorWindowRule
{
    public List<string> Check(VendorWindow item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor window.</summary>
public sealed record VendorWindowValidator(List<IVendorWindowRule> Rules)
{
    public List<string> Validate(VendorWindow item)
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

    public void RequireValid(VendorWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorWindow> items)
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

/// <summary>Functions of vendor window rules.</summary>
public static class VendorWindowRules
{
    public static VendorWindowValidator DefaultVendorWindowValidator()
    {
        return new VendorWindowValidator(
            new List<IVendorWindowRule>
            {
                new VendorWindowNameLengthRule(86400),
                new VendorWindowRequiredNameRule("vendor window needs an entry"),
                new VendorWindowFloorValueRangeRule(5.6, 3225.7),
                new VendorWindowMinimumRangeRule(1.1, 996.7),
                new VendorWindowCountryLengthRule(255),
            });
    }
}
