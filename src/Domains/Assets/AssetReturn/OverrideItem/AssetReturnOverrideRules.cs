namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>A rule a asset return override must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnOverrideRule
{
    List<string> Check(AssetReturnOverride item);
}

public sealed record AssetReturnOverrideRequiredLabelRule(string Message) : IAssetReturnOverrideRule
{
    public List<string> Check(AssetReturnOverride item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnOverrideRequiredSourceRule(string Message) : IAssetReturnOverrideRule
{
    public List<string> Check(AssetReturnOverride item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnOverrideSourceLengthRule(int MaxLength) : IAssetReturnOverrideRule
{
    public List<string> Check(AssetReturnOverride item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnOverrideLabelLengthRule(int MaxLength) : IAssetReturnOverrideRule
{
    public List<string> Check(AssetReturnOverride item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return override.</summary>
public sealed record AssetReturnOverrideValidator(List<IAssetReturnOverrideRule> Rules)
{
    public List<string> Validate(AssetReturnOverride item)
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

    public void RequireValid(AssetReturnOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnOverride> items)
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

/// <summary>Functions of asset return override rules.</summary>
public static class AssetReturnOverrideRules
{
    public static AssetReturnOverrideValidator DefaultAssetReturnOverrideValidator()
    {
        return new AssetReturnOverrideValidator(
            new List<IAssetReturnOverrideRule>
            {
                new AssetReturnOverrideRequiredLabelRule("asset return override needs an entry"),
                new AssetReturnOverrideRequiredSourceRule("asset return override needs an entry"),
                new AssetReturnOverrideSourceLengthRule(1000),
                new AssetReturnOverrideLabelLengthRule(120),
            });
    }
}
