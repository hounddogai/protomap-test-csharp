namespace Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>A rule a asset return cycle must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnCycleRule
{
    List<string> Check(AssetReturnCycle item);
}

public sealed record AssetReturnCycleCodeLengthRule(int MaxLength) : IAssetReturnCycleRule
{
    public List<string> Check(AssetReturnCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnCycleTagsLimitRule(int MaxCount, List<string> Banned) : IAssetReturnCycleRule
{
    public List<string> Check(AssetReturnCycle item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record AssetReturnCycleRequiredCodeRule(string Message) : IAssetReturnCycleRule
{
    public List<string> Check(AssetReturnCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnCycleUpdatedAtRangeRule(int Min, int Max) : IAssetReturnCycleRule
{
    public List<string> Check(AssetReturnCycle item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return cycle.</summary>
public sealed record AssetReturnCycleValidator(List<IAssetReturnCycleRule> Rules)
{
    public List<string> Validate(AssetReturnCycle item)
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

    public void RequireValid(AssetReturnCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnCycle> items)
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

/// <summary>Functions of asset return cycle rules.</summary>
public static class AssetReturnCycleRules
{
    public static AssetReturnCycleValidator DefaultAssetReturnCycleValidator()
    {
        return new AssetReturnCycleValidator(
            new List<IAssetReturnCycleRule>
            {
                new AssetReturnCycleCodeLengthRule(86400),
                new AssetReturnCycleTagsLimitRule(3600, new List<string> { "legacy", "global", "primary" }),
                new AssetReturnCycleRequiredCodeRule("asset return cycle needs a value"),
                new AssetReturnCycleUpdatedAtRangeRule(255, 3600),
            });
    }
}
