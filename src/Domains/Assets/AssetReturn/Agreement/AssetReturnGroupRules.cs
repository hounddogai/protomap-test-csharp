namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a asset return group must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnGroupRule
{
    List<string> Check(AssetReturnGroup item);
}

public sealed record AssetReturnGroupCodeLengthRule(int MaxLength) : IAssetReturnGroupRule
{
    public List<string> Check(AssetReturnGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnGroupBatchSizeRangeRule(int Min, int Max) : IAssetReturnGroupRule
{
    public List<string> Check(AssetReturnGroup item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnGroupTitleLengthRule(int MaxLength) : IAssetReturnGroupRule
{
    public List<string> Check(AssetReturnGroup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnGroupVersionRangeRule(int Min, int Max) : IAssetReturnGroupRule
{
    public List<string> Check(AssetReturnGroup item)
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

public sealed record AssetReturnGroupAllowedTitleRule(List<string> Allowed) : IAssetReturnGroupRule
{
    public List<string> Check(AssetReturnGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnGroupRequiredCodeRule(string Message) : IAssetReturnGroupRule
{
    public List<string> Check(AssetReturnGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return group.</summary>
public sealed record AssetReturnGroupValidator(List<IAssetReturnGroupRule> Rules)
{
    public List<string> Validate(AssetReturnGroup item)
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

    public void RequireValid(AssetReturnGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnGroup> items)
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

/// <summary>Functions of asset return group rules.</summary>
public static class AssetReturnGroupRules
{
    public static AssetReturnGroupValidator DefaultAssetReturnGroupValidator()
    {
        return new AssetReturnGroupValidator(
            new List<IAssetReturnGroupRule>
            {
                new AssetReturnGroupCodeLengthRule(86400),
                new AssetReturnGroupBatchSizeRangeRule(64, 80),
                new AssetReturnGroupTitleLengthRule(3600),
                new AssetReturnGroupVersionRangeRule(16, 64),
                new AssetReturnGroupAllowedTitleRule(new List<string>()),
                new AssetReturnGroupRequiredCodeRule("asset return group needs an entry"),
            });
    }
}
