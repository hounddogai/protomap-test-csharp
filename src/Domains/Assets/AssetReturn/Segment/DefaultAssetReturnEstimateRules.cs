namespace Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>A rule a default asset return estimate must pass. Check returns its problems, or none.</summary>
public interface IDefaultAssetReturnEstimateRule
{
    List<string> Check(DefaultAssetReturnEstimate item);
}

public sealed record DefaultAssetReturnEstimateMarginRangeRule(double Min, double Max) : IDefaultAssetReturnEstimateRule
{
    public List<string> Check(DefaultAssetReturnEstimate item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAssetReturnEstimateScoreRangeRule(double Min, double Max) : IDefaultAssetReturnEstimateRule
{
    public List<string> Check(DefaultAssetReturnEstimate item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultAssetReturnEstimateRequiredEmployeeIdRule(string Message) : IDefaultAssetReturnEstimateRule
{
    public List<string> Check(DefaultAssetReturnEstimate item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default asset return estimate.</summary>
public sealed record DefaultAssetReturnEstimateValidator(List<IDefaultAssetReturnEstimateRule> Rules)
{
    public List<string> Validate(DefaultAssetReturnEstimate item)
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

    public void RequireValid(DefaultAssetReturnEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAssetReturnEstimate> items)
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

/// <summary>Functions of default asset return estimate rules.</summary>
public static class DefaultAssetReturnEstimateRules
{
    public static DefaultAssetReturnEstimateValidator DefaultDefaultAssetReturnEstimateValidator()
    {
        return new DefaultAssetReturnEstimateValidator(
            new List<IDefaultAssetReturnEstimateRule>
            {
                new DefaultAssetReturnEstimateMarginRangeRule(3.0, 3098.1),
                new DefaultAssetReturnEstimateScoreRangeRule(6.8, 4824.0),
                new DefaultAssetReturnEstimateRequiredEmployeeIdRule("default asset return estimate needs a value"),
            });
    }
}
