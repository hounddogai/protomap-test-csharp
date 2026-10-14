namespace Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>A rule a asset return checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnCheckpointRule
{
    List<string> Check(AssetReturnCheckpoint item);
}

public sealed record AssetReturnCheckpointNameLengthRule(int MaxLength) : IAssetReturnCheckpointRule
{
    public List<string> Check(AssetReturnCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnCheckpointRequiredReviewerRule(string Message) : IAssetReturnCheckpointRule
{
    public List<string> Check(AssetReturnCheckpoint item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnCheckpointRequiredCodeRule(string Message) : IAssetReturnCheckpointRule
{
    public List<string> Check(AssetReturnCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return checkpoint.</summary>
public sealed record AssetReturnCheckpointValidator(List<IAssetReturnCheckpointRule> Rules)
{
    public List<string> Validate(AssetReturnCheckpoint item)
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

    public void RequireValid(AssetReturnCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnCheckpoint> items)
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

/// <summary>Functions of asset return checkpoint rules.</summary>
public static class AssetReturnCheckpointRules
{
    public static AssetReturnCheckpointValidator DefaultAssetReturnCheckpointValidator()
    {
        return new AssetReturnCheckpointValidator(
            new List<IAssetReturnCheckpointRule>
            {
                new AssetReturnCheckpointNameLengthRule(1000),
                new AssetReturnCheckpointRequiredReviewerRule("asset return checkpoint needs an entry"),
                new AssetReturnCheckpointRequiredCodeRule("asset return checkpoint needs this field"),
            });
    }
}
