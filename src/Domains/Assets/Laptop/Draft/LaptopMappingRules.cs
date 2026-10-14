namespace Acme.Workforce.Assets.Laptop.Draft;

/// <summary>A rule a laptop mapping must pass. Check returns its problems, or none.</summary>
public interface ILaptopMappingRule
{
    List<string> Check(LaptopMapping item);
}

public sealed record LaptopMappingCodeLengthRule(int MaxLength) : ILaptopMappingRule
{
    public List<string> Check(LaptopMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopMappingRequiredNameRule(string Message) : ILaptopMappingRule
{
    public List<string> Check(LaptopMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopMappingDepartmentLengthRule(int MaxLength) : ILaptopMappingRule
{
    public List<string> Check(LaptopMapping item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopMappingCodePrefixRule(string Prefix) : ILaptopMappingRule
{
    public List<string> Check(LaptopMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop mapping.</summary>
public sealed record LaptopMappingValidator(List<ILaptopMappingRule> Rules)
{
    public List<string> Validate(LaptopMapping item)
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

    public void RequireValid(LaptopMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopMapping> items)
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

/// <summary>Functions of laptop mapping rules.</summary>
public static class LaptopMappingRules
{
    public static LaptopMappingValidator DefaultLaptopMappingValidator()
    {
        return new LaptopMappingValidator(
            new List<ILaptopMappingRule>
            {
                new LaptopMappingCodeLengthRule(86400),
                new LaptopMappingRequiredNameRule("laptop mapping needs this field"),
                new LaptopMappingDepartmentLengthRule(32),
                new LaptopMappingCodePrefixRule("OPS"),
            });
    }
}
