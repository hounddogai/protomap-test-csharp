namespace Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>A rule a asset return window must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnWindowRule
{
    List<string> Check(AssetReturnWindow item);
}

public sealed record AssetReturnWindowVersionRangeRule(int Min, int Max) : IAssetReturnWindowRule
{
    public List<string> Check(AssetReturnWindow item)
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

public sealed record AssetReturnWindowRatioRangeRule(double Min, double Max) : IAssetReturnWindowRule
{
    public List<string> Check(AssetReturnWindow item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssetReturnWindowTitleLengthRule(int MaxLength) : IAssetReturnWindowRule
{
    public List<string> Check(AssetReturnWindow item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnWindowCodePrefixRule(string Prefix) : IAssetReturnWindowRule
{
    public List<string> Check(AssetReturnWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return window.</summary>
public sealed record AssetReturnWindowValidator(List<IAssetReturnWindowRule> Rules)
{
    public List<string> Validate(AssetReturnWindow item)
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

    public void RequireValid(AssetReturnWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnWindow> items)
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

/// <summary>Functions of asset return window rules.</summary>
public static class AssetReturnWindowRules
{
    public static AssetReturnWindowValidator DefaultAssetReturnWindowValidator()
    {
        return new AssetReturnWindowValidator(
            new List<IAssetReturnWindowRule>
            {
                new AssetReturnWindowVersionRangeRule(120, 1000),
                new AssetReturnWindowRatioRangeRule(1.1, 3214.1),
                new AssetReturnWindowTitleLengthRule(80),
                new AssetReturnWindowCodePrefixRule("X"),
            });
    }
}
