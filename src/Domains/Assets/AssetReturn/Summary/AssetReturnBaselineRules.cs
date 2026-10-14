namespace Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>A rule a asset return baseline must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnBaselineRule
{
    List<string> Check(AssetReturnBaseline item);
}

public sealed record AssetReturnBaselineCodePrefixRule(string Prefix) : IAssetReturnBaselineRule
{
    public List<string> Check(AssetReturnBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBaselineDepartmentLengthRule(int MaxLength) : IAssetReturnBaselineRule
{
    public List<string> Check(AssetReturnBaseline item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBaselineMinimumRangeRule(double Min, double Max) : IAssetReturnBaselineRule
{
    public List<string> Check(AssetReturnBaseline item)
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

public sealed record AssetReturnBaselineQuantityRangeRule(int Min, int Max) : IAssetReturnBaselineRule
{
    public List<string> Check(AssetReturnBaseline item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBaselineRequiredDepartmentRule(string Message) : IAssetReturnBaselineRule
{
    public List<string> Check(AssetReturnBaseline item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBaselineVersionRangeRule(int Min, int Max) : IAssetReturnBaselineRule
{
    public List<string> Check(AssetReturnBaseline item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a asset return baseline.</summary>
public sealed record AssetReturnBaselineValidator(List<IAssetReturnBaselineRule> Rules)
{
    public List<string> Validate(AssetReturnBaseline item)
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

    public void RequireValid(AssetReturnBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnBaseline> items)
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

/// <summary>Functions of asset return baseline rules.</summary>
public static class AssetReturnBaselineRules
{
    public static AssetReturnBaselineValidator DefaultAssetReturnBaselineValidator()
    {
        return new AssetReturnBaselineValidator(
            new List<IAssetReturnBaselineRule>
            {
                new AssetReturnBaselineCodePrefixRule("OPS"),
                new AssetReturnBaselineDepartmentLengthRule(3600),
                new AssetReturnBaselineMinimumRangeRule(3.1, 1914.7),
                new AssetReturnBaselineQuantityRangeRule(16, 64),
                new AssetReturnBaselineRequiredDepartmentRule("asset return baseline needs an entry"),
                new AssetReturnBaselineVersionRangeRule(1000, 1000),
            });
    }
}
