namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive warranty mapping must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveWarrantyMappingRule
{
    List<string> Check(AdaptiveWarrantyMapping item);
}

public sealed record AdaptiveWarrantyMappingQuantityRangeRule(int Min, int Max) : IAdaptiveWarrantyMappingRule
{
    public List<string> Check(AdaptiveWarrantyMapping item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyMappingLabelLengthRule(int MaxLength) : IAdaptiveWarrantyMappingRule
{
    public List<string> Check(AdaptiveWarrantyMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyMappingRatioRangeRule(double Min, double Max) : IAdaptiveWarrantyMappingRule
{
    public List<string> Check(AdaptiveWarrantyMapping item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyMappingExpiresAtRangeRule(int Min, int Max) : IAdaptiveWarrantyMappingRule
{
    public List<string> Check(AdaptiveWarrantyMapping item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyMappingAllowedLabelRule(List<string> Allowed) : IAdaptiveWarrantyMappingRule
{
    public List<string> Check(AdaptiveWarrantyMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyMappingCodeLengthRule(int MaxLength) : IAdaptiveWarrantyMappingRule
{
    public List<string> Check(AdaptiveWarrantyMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive warranty mapping.</summary>
public sealed record AdaptiveWarrantyMappingValidator(List<IAdaptiveWarrantyMappingRule> Rules)
{
    public List<string> Validate(AdaptiveWarrantyMapping item)
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

    public void RequireValid(AdaptiveWarrantyMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveWarrantyMapping> items)
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

/// <summary>Functions of adaptive warranty mapping rules.</summary>
public static class AdaptiveWarrantyMappingRules
{
    public static AdaptiveWarrantyMappingValidator DefaultAdaptiveWarrantyMappingValidator()
    {
        return new AdaptiveWarrantyMappingValidator(
            new List<IAdaptiveWarrantyMappingRule>
            {
                new AdaptiveWarrantyMappingQuantityRangeRule(80, 255),
                new AdaptiveWarrantyMappingLabelLengthRule(32),
                new AdaptiveWarrantyMappingRatioRangeRule(7.7, 4686.2),
                new AdaptiveWarrantyMappingExpiresAtRangeRule(64, 1000),
                new AdaptiveWarrantyMappingAllowedLabelRule(new List<string> { "manual", "regional", "capped" }),
                new AdaptiveWarrantyMappingCodeLengthRule(16),
            });
    }
}
