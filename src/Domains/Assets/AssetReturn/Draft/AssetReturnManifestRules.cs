namespace Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>A rule a asset return manifest must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnManifestRule
{
    List<string> Check(AssetReturnManifest item);
}

public sealed record AssetReturnManifestRequiredLocationRule(string Message) : IAssetReturnManifestRule
{
    public List<string> Check(AssetReturnManifest item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnManifestLabelLengthRule(int MaxLength) : IAssetReturnManifestRule
{
    public List<string> Check(AssetReturnManifest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnManifestRequiredTimezoneRule(string Message) : IAssetReturnManifestRule
{
    public List<string> Check(AssetReturnManifest item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnManifestApprovedGuardRule(double Limit) : IAssetReturnManifestRule
{
    public List<string> Check(AssetReturnManifest item)
    {
        if (item.Approved && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return manifest.</summary>
public sealed record AssetReturnManifestValidator(List<IAssetReturnManifestRule> Rules)
{
    public List<string> Validate(AssetReturnManifest item)
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

    public void RequireValid(AssetReturnManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnManifest> items)
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

/// <summary>Functions of asset return manifest rules.</summary>
public static class AssetReturnManifestRules
{
    public static AssetReturnManifestValidator DefaultAssetReturnManifestValidator()
    {
        return new AssetReturnManifestValidator(
            new List<IAssetReturnManifestRule>
            {
                new AssetReturnManifestRequiredLocationRule("asset return manifest needs this field"),
                new AssetReturnManifestLabelLengthRule(32),
                new AssetReturnManifestRequiredTimezoneRule("asset return manifest needs an entry"),
                new AssetReturnManifestApprovedGuardRule(6.4),
            });
    }
}
