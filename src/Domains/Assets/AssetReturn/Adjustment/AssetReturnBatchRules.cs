namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

/// <summary>A rule a asset return batch must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnBatchRule
{
    List<string> Check(AssetReturnBatch item);
}

public sealed record AssetReturnBatchTitleLengthRule(int MaxLength) : IAssetReturnBatchRule
{
    public List<string> Check(AssetReturnBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBatchRequiredTitleRule(string Message) : IAssetReturnBatchRule
{
    public List<string> Check(AssetReturnBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBatchCapacityRangeRule(int Min, int Max) : IAssetReturnBatchRule
{
    public List<string> Check(AssetReturnBatch item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssetReturnBatchRequiredCodeRule(string Message) : IAssetReturnBatchRule
{
    public List<string> Check(AssetReturnBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBatchNameLengthRule(int MaxLength) : IAssetReturnBatchRule
{
    public List<string> Check(AssetReturnBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBatchRequiredNameRule(string Message) : IAssetReturnBatchRule
{
    public List<string> Check(AssetReturnBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return batch.</summary>
public sealed record AssetReturnBatchValidator(List<IAssetReturnBatchRule> Rules)
{
    public List<string> Validate(AssetReturnBatch item)
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

    public void RequireValid(AssetReturnBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnBatch> items)
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

/// <summary>Functions of asset return batch rules.</summary>
public static class AssetReturnBatchRules
{
    public static AssetReturnBatchValidator DefaultAssetReturnBatchValidator()
    {
        return new AssetReturnBatchValidator(
            new List<IAssetReturnBatchRule>
            {
                new AssetReturnBatchTitleLengthRule(255),
                new AssetReturnBatchRequiredTitleRule("asset return batch needs this field"),
                new AssetReturnBatchCapacityRangeRule(64, 80),
                new AssetReturnBatchRequiredCodeRule("asset return batch needs an entry"),
                new AssetReturnBatchNameLengthRule(80),
                new AssetReturnBatchRequiredNameRule("asset return batch needs this field"),
            });
    }
}
