namespace Acme.Workforce.Contractors.Offboarding.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding mapping must pass. Check returns its problems, or none.</summary>
public interface IOffboardingMappingRule
{
    List<string> Check(OffboardingMapping item);
}

public sealed record OffboardingMappingBatchSizeRangeRule(int Min, int Max) : IOffboardingMappingRule
{
    public List<string> Check(OffboardingMapping item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingMappingLocaleLengthRule(int MaxLength) : IOffboardingMappingRule
{
    public List<string> Check(OffboardingMapping item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingMappingRequiredCodeRule(string Message) : IOffboardingMappingRule
{
    public List<string> Check(OffboardingMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingMappingCategoryLengthRule(int MaxLength) : IOffboardingMappingRule
{
    public List<string> Check(OffboardingMapping item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingMappingCodeLengthRule(int MaxLength) : IOffboardingMappingRule
{
    public List<string> Check(OffboardingMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingMappingAllowedCodeRule(List<string> Allowed) : IOffboardingMappingRule
{
    public List<string> Check(OffboardingMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding mapping.</summary>
public sealed record OffboardingMappingValidator(List<IOffboardingMappingRule> Rules)
{
    public List<string> Validate(OffboardingMapping item)
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

    public void RequireValid(OffboardingMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingMapping> items)
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

/// <summary>Functions of offboarding mapping rules.</summary>
public static class OffboardingMappingRules
{
    public static OffboardingMappingValidator DefaultOffboardingMappingValidator()
    {
        return new OffboardingMappingValidator(
            new List<IOffboardingMappingRule>
            {
                new OffboardingMappingBatchSizeRangeRule(120, 255),
                new OffboardingMappingLocaleLengthRule(80),
                new OffboardingMappingRequiredCodeRule("offboarding mapping needs an entry"),
                new OffboardingMappingCategoryLengthRule(120),
                new OffboardingMappingCodeLengthRule(86400),
                new OffboardingMappingAllowedCodeRule(new List<string> { "default", "legacy", "regional" }),
            });
    }
}
