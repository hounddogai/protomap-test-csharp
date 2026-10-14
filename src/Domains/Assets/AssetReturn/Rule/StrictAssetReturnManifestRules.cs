namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict asset return manifest must pass. Check returns its problems, or none.</summary>
public interface IStrictAssetReturnManifestRule
{
    List<string> Check(StrictAssetReturnManifest item);
}

public sealed record StrictAssetReturnManifestRequiredNameRule(string Message) : IStrictAssetReturnManifestRule
{
    public List<string> Check(StrictAssetReturnManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAssetReturnManifestAmountRangeRule(double Min, double Max) : IStrictAssetReturnManifestRule
{
    public List<string> Check(StrictAssetReturnManifest item)
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

public sealed record StrictAssetReturnManifestAllowedCodeRule(List<string> Allowed) : IStrictAssetReturnManifestRule
{
    public List<string> Check(StrictAssetReturnManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict asset return manifest.</summary>
public sealed record StrictAssetReturnManifestValidator(List<IStrictAssetReturnManifestRule> Rules)
{
    public List<string> Validate(StrictAssetReturnManifest item)
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

    public void RequireValid(StrictAssetReturnManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAssetReturnManifest> items)
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

/// <summary>Functions of strict asset return manifest rules.</summary>
public static class StrictAssetReturnManifestRules
{
    public static StrictAssetReturnManifestValidator DefaultStrictAssetReturnManifestValidator()
    {
        return new StrictAssetReturnManifestValidator(
            new List<IStrictAssetReturnManifestRule>
            {
                new StrictAssetReturnManifestRequiredNameRule("strict asset return manifest needs a value"),
                new StrictAssetReturnManifestAmountRangeRule(3.9, 3415.8),
                new StrictAssetReturnManifestAllowedCodeRule(new List<string> { "manual" }),
            });
    }
}
