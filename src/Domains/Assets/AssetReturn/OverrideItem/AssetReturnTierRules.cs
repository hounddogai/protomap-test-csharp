namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>A rule a asset return tier must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnTierRule
{
    List<string> Check(AssetReturnTier item);
}

public sealed record AssetReturnTierRequiredRegionRule(string Message) : IAssetReturnTierRule
{
    public List<string> Check(AssetReturnTier item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnTierAmountRangeRule(double Min, double Max) : IAssetReturnTierRule
{
    public List<string> Check(AssetReturnTier item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssetReturnTierRequiredLabelRule(string Message) : IAssetReturnTierRule
{
    public List<string> Check(AssetReturnTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return tier.</summary>
public sealed record AssetReturnTierValidator(List<IAssetReturnTierRule> Rules)
{
    public List<string> Validate(AssetReturnTier item)
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

    public void RequireValid(AssetReturnTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnTier> items)
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

/// <summary>Functions of asset return tier rules.</summary>
public static class AssetReturnTierRules
{
    public static AssetReturnTierValidator DefaultAssetReturnTierValidator()
    {
        return new AssetReturnTierValidator(
            new List<IAssetReturnTierRule>
            {
                new AssetReturnTierRequiredRegionRule("asset return tier needs a value"),
                new AssetReturnTierAmountRangeRule(3.2, 2205.9),
                new AssetReturnTierRequiredLabelRule("asset return tier needs this field"),
            });
    }
}
