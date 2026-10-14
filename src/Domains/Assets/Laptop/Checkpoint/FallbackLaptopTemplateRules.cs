namespace Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>A rule a fallback laptop template must pass. Check returns its problems, or none.</summary>
public interface IFallbackLaptopTemplateRule
{
    List<string> Check(FallbackLaptopTemplate item);
}

public sealed record FallbackLaptopTemplateRequiredTitleRule(string Message) : IFallbackLaptopTemplateRule
{
    public List<string> Check(FallbackLaptopTemplate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackLaptopTemplateCodePrefixRule(string Prefix) : IFallbackLaptopTemplateRule
{
    public List<string> Check(FallbackLaptopTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLaptopTemplateTitleLengthRule(int MaxLength) : IFallbackLaptopTemplateRule
{
    public List<string> Check(FallbackLaptopTemplate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback laptop template.</summary>
public sealed record FallbackLaptopTemplateValidator(List<IFallbackLaptopTemplateRule> Rules)
{
    public List<string> Validate(FallbackLaptopTemplate item)
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

    public void RequireValid(FallbackLaptopTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackLaptopTemplate> items)
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

/// <summary>Functions of fallback laptop template rules.</summary>
public static class FallbackLaptopTemplateRules
{
    public static FallbackLaptopTemplateValidator DefaultFallbackLaptopTemplateValidator()
    {
        return new FallbackLaptopTemplateValidator(
            new List<IFallbackLaptopTemplateRule>
            {
                new FallbackLaptopTemplateRequiredTitleRule("fallback laptop template needs this field"),
                new FallbackLaptopTemplateCodePrefixRule("X"),
                new FallbackLaptopTemplateTitleLengthRule(120),
            });
    }
}
