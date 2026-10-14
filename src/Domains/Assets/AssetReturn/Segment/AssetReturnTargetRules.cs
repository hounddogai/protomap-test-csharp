namespace Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>A rule a asset return target must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnTargetRule
{
    List<string> Check(AssetReturnTarget item);
}

public sealed record AssetReturnTargetRequiredLabelRule(string Message) : IAssetReturnTargetRule
{
    public List<string> Check(AssetReturnTarget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnTargetCodePrefixRule(string Prefix) : IAssetReturnTargetRule
{
    public List<string> Check(AssetReturnTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnTargetRequiredContactEmailRule(string Message) : IAssetReturnTargetRule
{
    public List<string> Check(AssetReturnTarget item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return target.</summary>
public sealed record AssetReturnTargetValidator(List<IAssetReturnTargetRule> Rules)
{
    public List<string> Validate(AssetReturnTarget item)
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

    public void RequireValid(AssetReturnTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnTarget> items)
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

/// <summary>Functions of asset return target rules.</summary>
public static class AssetReturnTargetRules
{
    public static AssetReturnTargetValidator DefaultAssetReturnTargetValidator()
    {
        return new AssetReturnTargetValidator(
            new List<IAssetReturnTargetRule>
            {
                new AssetReturnTargetRequiredLabelRule("asset return target needs a value"),
                new AssetReturnTargetCodePrefixRule("Q"),
                new AssetReturnTargetRequiredContactEmailRule("asset return target needs an entry"),
            });
    }
}
