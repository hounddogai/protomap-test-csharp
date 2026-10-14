namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy laptop template must pass. Check returns its problems, or none.</summary>
public interface ILegacyLaptopTemplateRule
{
    List<string> Check(LegacyLaptopTemplate item);
}

public sealed record LegacyLaptopTemplateQuantityRangeRule(int Min, int Max) : ILegacyLaptopTemplateRule
{
    public List<string> Check(LegacyLaptopTemplate item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyLaptopTemplateRequiredCodeRule(string Message) : ILegacyLaptopTemplateRule
{
    public List<string> Check(LegacyLaptopTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyLaptopTemplateApprovedGuardRule(double Limit) : ILegacyLaptopTemplateRule
{
    public List<string> Check(LegacyLaptopTemplate item)
    {
        if (item.Approved && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLaptopTemplateAllowedCodeRule(List<string> Allowed) : ILegacyLaptopTemplateRule
{
    public List<string> Check(LegacyLaptopTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLaptopTemplateBadgeLengthRule(int MaxLength) : ILegacyLaptopTemplateRule
{
    public List<string> Check(LegacyLaptopTemplate item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy laptop template.</summary>
public sealed record LegacyLaptopTemplateValidator(List<ILegacyLaptopTemplateRule> Rules)
{
    public List<string> Validate(LegacyLaptopTemplate item)
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

    public void RequireValid(LegacyLaptopTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyLaptopTemplate> items)
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

/// <summary>Functions of legacy laptop template rules.</summary>
public static class LegacyLaptopTemplateRules
{
    public static LegacyLaptopTemplateValidator DefaultLegacyLaptopTemplateValidator()
    {
        return new LegacyLaptopTemplateValidator(
            new List<ILegacyLaptopTemplateRule>
            {
                new LegacyLaptopTemplateQuantityRangeRule(80, 86400),
                new LegacyLaptopTemplateRequiredCodeRule("legacy laptop template needs this field"),
                new LegacyLaptopTemplateApprovedGuardRule(0.7),
                new LegacyLaptopTemplateAllowedCodeRule(new List<string>()),
                new LegacyLaptopTemplateBadgeLengthRule(64),
            });
    }
}
