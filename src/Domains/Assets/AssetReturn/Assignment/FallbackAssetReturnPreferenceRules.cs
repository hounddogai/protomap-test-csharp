namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback asset return preference must pass. Check returns its problems, or none.</summary>
public interface IFallbackAssetReturnPreferenceRule
{
    List<string> Check(FallbackAssetReturnPreference item);
}

public sealed record FallbackAssetReturnPreferenceRequiredTitleRule(string Message) : IFallbackAssetReturnPreferenceRule
{
    public List<string> Check(FallbackAssetReturnPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnPreferenceRequiredCodeRule(string Message) : IFallbackAssetReturnPreferenceRule
{
    public List<string> Check(FallbackAssetReturnPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnPreferenceAllowedLocationRule(List<string> Allowed) : IFallbackAssetReturnPreferenceRule
{
    public List<string> Check(FallbackAssetReturnPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback asset return preference.</summary>
public sealed record FallbackAssetReturnPreferenceValidator(List<IFallbackAssetReturnPreferenceRule> Rules)
{
    public List<string> Validate(FallbackAssetReturnPreference item)
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

    public void RequireValid(FallbackAssetReturnPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAssetReturnPreference> items)
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

/// <summary>Functions of fallback asset return preference rules.</summary>
public static class FallbackAssetReturnPreferenceRules
{
    public static FallbackAssetReturnPreferenceValidator DefaultFallbackAssetReturnPreferenceValidator()
    {
        return new FallbackAssetReturnPreferenceValidator(
            new List<IFallbackAssetReturnPreferenceRule>
            {
                new FallbackAssetReturnPreferenceRequiredTitleRule("fallback asset return preference needs an entry"),
                new FallbackAssetReturnPreferenceRequiredCodeRule(
                    "fallback asset return preference needs this field"),
                new FallbackAssetReturnPreferenceAllowedLocationRule(new List<string> { "capped" }),
            });
    }
}
