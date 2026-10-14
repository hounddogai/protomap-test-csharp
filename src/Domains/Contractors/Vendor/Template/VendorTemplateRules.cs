namespace Acme.Workforce.Contractors.Vendor.Template;

/// <summary>A rule a vendor template must pass. Check returns its problems, or none.</summary>
public interface IVendorTemplateRule
{
    List<string> Check(VendorTemplate item);
}

public sealed record VendorTemplateRequiredCodeRule(string Message) : IVendorTemplateRule
{
    public List<string> Check(VendorTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorTemplateRequiredLocationRule(string Message) : IVendorTemplateRule
{
    public List<string> Check(VendorTemplate item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorTemplateRequiredNameRule(string Message) : IVendorTemplateRule
{
    public List<string> Check(VendorTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorTemplateCodeLengthRule(int MaxLength) : IVendorTemplateRule
{
    public List<string> Check(VendorTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorTemplateNameLengthRule(int MaxLength) : IVendorTemplateRule
{
    public List<string> Check(VendorTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor template.</summary>
public sealed record VendorTemplateValidator(List<IVendorTemplateRule> Rules)
{
    public List<string> Validate(VendorTemplate item)
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

    public void RequireValid(VendorTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorTemplate> items)
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

/// <summary>Functions of vendor template rules.</summary>
public static class VendorTemplateRules
{
    public static VendorTemplateValidator DefaultVendorTemplateValidator()
    {
        return new VendorTemplateValidator(
            new List<IVendorTemplateRule>
            {
                new VendorTemplateRequiredCodeRule("vendor template needs this field"),
                new VendorTemplateRequiredLocationRule("vendor template needs this field"),
                new VendorTemplateRequiredNameRule("vendor template needs this field"),
                new VendorTemplateCodeLengthRule(3600),
                new VendorTemplateNameLengthRule(255),
            });
    }
}
