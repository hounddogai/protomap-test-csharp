namespace Acme.Workforce.Assets.AssetReturn.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual asset return tier must pass. Check returns its problems, or none.</summary>
public interface IManualAssetReturnTierRule
{
    List<string> Check(ManualAssetReturnTier item);
}

public sealed record ManualAssetReturnTierRequiredTitleRule(string Message) : IManualAssetReturnTierRule
{
    public List<string> Check(ManualAssetReturnTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAssetReturnTierRequiredCodeRule(string Message) : IManualAssetReturnTierRule
{
    public List<string> Check(ManualAssetReturnTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAssetReturnTierCodeLengthRule(int MaxLength) : IManualAssetReturnTierRule
{
    public List<string> Check(ManualAssetReturnTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssetReturnTierCodePrefixRule(string Prefix) : IManualAssetReturnTierRule
{
    public List<string> Check(ManualAssetReturnTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssetReturnTierAllowedTitleRule(List<string> Allowed) : IManualAssetReturnTierRule
{
    public List<string> Check(ManualAssetReturnTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssetReturnTierCreatedAtRangeRule(int Min, int Max) : IManualAssetReturnTierRule
{
    public List<string> Check(ManualAssetReturnTier item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a manual asset return tier.</summary>
public sealed record ManualAssetReturnTierValidator(List<IManualAssetReturnTierRule> Rules)
{
    public List<string> Validate(ManualAssetReturnTier item)
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

    public void RequireValid(ManualAssetReturnTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAssetReturnTier> items)
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

/// <summary>Functions of manual asset return tier rules.</summary>
public static class ManualAssetReturnTierRules
{
    public static ManualAssetReturnTierValidator DefaultManualAssetReturnTierValidator()
    {
        return new ManualAssetReturnTierValidator(
            new List<IManualAssetReturnTierRule>
            {
                new ManualAssetReturnTierRequiredTitleRule("manual asset return tier needs this field"),
                new ManualAssetReturnTierRequiredCodeRule("manual asset return tier needs a value"),
                new ManualAssetReturnTierCodeLengthRule(32),
                new ManualAssetReturnTierCodePrefixRule("OPS"),
                new ManualAssetReturnTierAllowedTitleRule(new List<string> { "legacy" }),
                new ManualAssetReturnTierCreatedAtRangeRule(80, 1000),
            });
    }
}
