namespace Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>A rule a asset return profile must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnProfileRule
{
    List<string> Check(AssetReturnProfile item);
}

public sealed record AssetReturnProfileFteRangeRule(double Min, double Max) : IAssetReturnProfileRule
{
    public List<string> Check(AssetReturnProfile item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnProfileRequiredTitleRule(string Message) : IAssetReturnProfileRule
{
    public List<string> Check(AssetReturnProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnProfileDiscountRangeRule(double Min, double Max) : IAssetReturnProfileRule
{
    public List<string> Check(AssetReturnProfile item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnProfileRequiredReviewerRule(string Message) : IAssetReturnProfileRule
{
    public List<string> Check(AssetReturnProfile item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnProfileMarginRangeRule(double Min, double Max) : IAssetReturnProfileRule
{
    public List<string> Check(AssetReturnProfile item)
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

public sealed record AssetReturnProfileRequiredCodeRule(string Message) : IAssetReturnProfileRule
{
    public List<string> Check(AssetReturnProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return profile.</summary>
public sealed record AssetReturnProfileValidator(List<IAssetReturnProfileRule> Rules)
{
    public List<string> Validate(AssetReturnProfile item)
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

    public void RequireValid(AssetReturnProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnProfile> items)
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

/// <summary>Functions of asset return profile rules.</summary>
public static class AssetReturnProfileRules
{
    public static AssetReturnProfileValidator DefaultAssetReturnProfileValidator()
    {
        return new AssetReturnProfileValidator(
            new List<IAssetReturnProfileRule>
            {
                new AssetReturnProfileFteRangeRule(0.0, 1858.6),
                new AssetReturnProfileRequiredTitleRule("asset return profile needs an entry"),
                new AssetReturnProfileDiscountRangeRule(7.8, 3851.7),
                new AssetReturnProfileRequiredReviewerRule("asset return profile needs a value"),
                new AssetReturnProfileMarginRangeRule(8.4, 4822.0),
                new AssetReturnProfileRequiredCodeRule("asset return profile needs an entry"),
            });
    }
}
