namespace Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>A rule a asset return summary must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnSummaryRule
{
    List<string> Check(AssetReturnSummary item);
}

public sealed record AssetReturnSummaryCodePrefixRule(string Prefix) : IAssetReturnSummaryRule
{
    public List<string> Check(AssetReturnSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnSummarySegmentLengthRule(int MaxLength) : IAssetReturnSummaryRule
{
    public List<string> Check(AssetReturnSummary item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnSummaryScoreRangeRule(double Min, double Max) : IAssetReturnSummaryRule
{
    public List<string> Check(AssetReturnSummary item)
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

public sealed record AssetReturnSummaryLabelLengthRule(int MaxLength) : IAssetReturnSummaryRule
{
    public List<string> Check(AssetReturnSummary item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnSummaryVisibleGuardRule(double Limit) : IAssetReturnSummaryRule
{
    public List<string> Check(AssetReturnSummary item)
    {
        if (item.Visible && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return summary.</summary>
public sealed record AssetReturnSummaryValidator(List<IAssetReturnSummaryRule> Rules)
{
    public List<string> Validate(AssetReturnSummary item)
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

    public void RequireValid(AssetReturnSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnSummary> items)
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

/// <summary>Functions of asset return summary rules.</summary>
public static class AssetReturnSummaryRules
{
    public static AssetReturnSummaryValidator DefaultAssetReturnSummaryValidator()
    {
        return new AssetReturnSummaryValidator(
            new List<IAssetReturnSummaryRule>
            {
                new AssetReturnSummaryCodePrefixRule("ACM"),
                new AssetReturnSummarySegmentLengthRule(120),
                new AssetReturnSummaryScoreRangeRule(9.8, 2723.3),
                new AssetReturnSummaryLabelLengthRule(16),
                new AssetReturnSummaryVisibleGuardRule(6.7),
            });
    }
}
