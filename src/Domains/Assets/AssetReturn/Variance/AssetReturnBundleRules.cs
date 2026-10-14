namespace Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>A rule a asset return bundle must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnBundleRule
{
    List<string> Check(AssetReturnBundle item);
}

public sealed record AssetReturnBundleCostRangeRule(double Min, double Max) : IAssetReturnBundleRule
{
    public List<string> Check(AssetReturnBundle item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssetReturnBundleCodeLengthRule(int MaxLength) : IAssetReturnBundleRule
{
    public List<string> Check(AssetReturnBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBundleRequiredNameRule(string Message) : IAssetReturnBundleRule
{
    public List<string> Check(AssetReturnBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBundleActiveGuardRule(int Limit) : IAssetReturnBundleRule
{
    public List<string> Check(AssetReturnBundle item)
    {
        if (item.Active && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBundleRequiredDepartmentRule(string Message) : IAssetReturnBundleRule
{
    public List<string> Check(AssetReturnBundle item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBundleDepartmentLengthRule(int MaxLength) : IAssetReturnBundleRule
{
    public List<string> Check(AssetReturnBundle item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return bundle.</summary>
public sealed record AssetReturnBundleValidator(List<IAssetReturnBundleRule> Rules)
{
    public List<string> Validate(AssetReturnBundle item)
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

    public void RequireValid(AssetReturnBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnBundle> items)
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

/// <summary>Functions of asset return bundle rules.</summary>
public static class AssetReturnBundleRules
{
    public static AssetReturnBundleValidator DefaultAssetReturnBundleValidator()
    {
        return new AssetReturnBundleValidator(
            new List<IAssetReturnBundleRule>
            {
                new AssetReturnBundleCostRangeRule(7.1, 4928.4),
                new AssetReturnBundleCodeLengthRule(120),
                new AssetReturnBundleRequiredNameRule("asset return bundle needs this field"),
                new AssetReturnBundleActiveGuardRule(86400),
                new AssetReturnBundleRequiredDepartmentRule("asset return bundle needs this field"),
                new AssetReturnBundleDepartmentLengthRule(64),
            });
    }
}
