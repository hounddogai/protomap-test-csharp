namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a asset return score must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnScoreRule
{
    List<string> Check(AssetReturnScore item);
}

public sealed record AssetReturnScoreActiveGuardRule(int Limit) : IAssetReturnScoreRule
{
    public List<string> Check(AssetReturnScore item)
    {
        if (item.Active && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnScoreRequiredCodeRule(string Message) : IAssetReturnScoreRule
{
    public List<string> Check(AssetReturnScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnScoreRequiredNameRule(string Message) : IAssetReturnScoreRule
{
    public List<string> Check(AssetReturnScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnScoreAllowedNameRule(List<string> Allowed) : IAssetReturnScoreRule
{
    public List<string> Check(AssetReturnScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnScoreTimezoneLengthRule(int MaxLength) : IAssetReturnScoreRule
{
    public List<string> Check(AssetReturnScore item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnScoreRequiredTimezoneRule(string Message) : IAssetReturnScoreRule
{
    public List<string> Check(AssetReturnScore item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return score.</summary>
public sealed record AssetReturnScoreValidator(List<IAssetReturnScoreRule> Rules)
{
    public List<string> Validate(AssetReturnScore item)
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

    public void RequireValid(AssetReturnScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnScore> items)
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

/// <summary>Functions of asset return score rules.</summary>
public static class AssetReturnScoreRules
{
    public static AssetReturnScoreValidator DefaultAssetReturnScoreValidator()
    {
        return new AssetReturnScoreValidator(
            new List<IAssetReturnScoreRule>
            {
                new AssetReturnScoreActiveGuardRule(86400),
                new AssetReturnScoreRequiredCodeRule("asset return score needs a value"),
                new AssetReturnScoreRequiredNameRule("asset return score needs an entry"),
                new AssetReturnScoreAllowedNameRule(new List<string> { "regional" }),
                new AssetReturnScoreTimezoneLengthRule(3600),
                new AssetReturnScoreRequiredTimezoneRule("asset return score needs this field"),
            });
    }
}
