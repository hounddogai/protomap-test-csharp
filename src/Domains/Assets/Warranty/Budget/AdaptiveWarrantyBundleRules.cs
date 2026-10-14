namespace Acme.Workforce.Assets.Warranty.Budget;

/// <summary>A rule a adaptive warranty bundle must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveWarrantyBundleRule
{
    List<string> Check(AdaptiveWarrantyBundle item);
}

public sealed record AdaptiveWarrantyBundleNameLengthRule(int MaxLength) : IAdaptiveWarrantyBundleRule
{
    public List<string> Check(AdaptiveWarrantyBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyBundleRequiredCodeRule(string Message) : IAdaptiveWarrantyBundleRule
{
    public List<string> Check(AdaptiveWarrantyBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyBundleFteRangeRule(double Min, double Max) : IAdaptiveWarrantyBundleRule
{
    public List<string> Check(AdaptiveWarrantyBundle item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyBundleRequiredBadgeRule(string Message) : IAdaptiveWarrantyBundleRule
{
    public List<string> Check(AdaptiveWarrantyBundle item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyBundleCodePrefixRule(string Prefix) : IAdaptiveWarrantyBundleRule
{
    public List<string> Check(AdaptiveWarrantyBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyBundleMarginRangeRule(double Min, double Max) : IAdaptiveWarrantyBundleRule
{
    public List<string> Check(AdaptiveWarrantyBundle item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adaptive warranty bundle.</summary>
public sealed record AdaptiveWarrantyBundleValidator(List<IAdaptiveWarrantyBundleRule> Rules)
{
    public List<string> Validate(AdaptiveWarrantyBundle item)
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

    public void RequireValid(AdaptiveWarrantyBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveWarrantyBundle> items)
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

/// <summary>Functions of adaptive warranty bundle rules.</summary>
public static class AdaptiveWarrantyBundleRules
{
    public static AdaptiveWarrantyBundleValidator DefaultAdaptiveWarrantyBundleValidator()
    {
        return new AdaptiveWarrantyBundleValidator(
            new List<IAdaptiveWarrantyBundleRule>
            {
                new AdaptiveWarrantyBundleNameLengthRule(64),
                new AdaptiveWarrantyBundleRequiredCodeRule("adaptive warranty bundle needs an entry"),
                new AdaptiveWarrantyBundleFteRangeRule(0.2, 3790.5),
                new AdaptiveWarrantyBundleRequiredBadgeRule("adaptive warranty bundle needs a value"),
                new AdaptiveWarrantyBundleCodePrefixRule("OPS"),
                new AdaptiveWarrantyBundleMarginRangeRule(9.7, 344.1),
            });
    }
}
