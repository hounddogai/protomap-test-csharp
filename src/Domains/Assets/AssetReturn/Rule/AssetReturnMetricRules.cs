namespace Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>A rule a asset return metric must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnMetricRule
{
    List<string> Check(AssetReturnMetric item);
}

public sealed record AssetReturnMetricCodeLengthRule(int MaxLength) : IAssetReturnMetricRule
{
    public List<string> Check(AssetReturnMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnMetricRequiredReviewerRule(string Message) : IAssetReturnMetricRule
{
    public List<string> Check(AssetReturnMetric item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnMetricTitleLengthRule(int MaxLength) : IAssetReturnMetricRule
{
    public List<string> Check(AssetReturnMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return metric.</summary>
public sealed record AssetReturnMetricValidator(List<IAssetReturnMetricRule> Rules)
{
    public List<string> Validate(AssetReturnMetric item)
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

    public void RequireValid(AssetReturnMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnMetric> items)
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

/// <summary>Functions of asset return metric rules.</summary>
public static class AssetReturnMetricRules
{
    public static AssetReturnMetricValidator DefaultAssetReturnMetricValidator()
    {
        return new AssetReturnMetricValidator(
            new List<IAssetReturnMetricRule>
            {
                new AssetReturnMetricCodeLengthRule(86400),
                new AssetReturnMetricRequiredReviewerRule("asset return metric needs an entry"),
                new AssetReturnMetricTitleLengthRule(80),
            });
    }
}
