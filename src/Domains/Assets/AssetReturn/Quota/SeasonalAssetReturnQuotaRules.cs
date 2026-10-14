namespace Acme.Workforce.Assets.AssetReturn.Quota;

/// <summary>A rule a seasonal asset return quota must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAssetReturnQuotaRule
{
    List<string> Check(SeasonalAssetReturnQuota item);
}

public sealed record SeasonalAssetReturnQuotaRequiredTitleRule(string Message) : ISeasonalAssetReturnQuotaRule
{
    public List<string> Check(SeasonalAssetReturnQuota item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAssetReturnQuotaReviewerLengthRule(int MaxLength) : ISeasonalAssetReturnQuotaRule
{
    public List<string> Check(SeasonalAssetReturnQuota item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAssetReturnQuotaCodePrefixRule(string Prefix) : ISeasonalAssetReturnQuotaRule
{
    public List<string> Check(SeasonalAssetReturnQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAssetReturnQuotaRatioRangeRule(double Min, double Max) : ISeasonalAssetReturnQuotaRule
{
    public List<string> Check(SeasonalAssetReturnQuota item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a seasonal asset return quota.</summary>
public sealed record SeasonalAssetReturnQuotaValidator(List<ISeasonalAssetReturnQuotaRule> Rules)
{
    public List<string> Validate(SeasonalAssetReturnQuota item)
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

    public void RequireValid(SeasonalAssetReturnQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAssetReturnQuota> items)
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

/// <summary>Functions of seasonal asset return quota rules.</summary>
public static class SeasonalAssetReturnQuotaRules
{
    public static SeasonalAssetReturnQuotaValidator DefaultSeasonalAssetReturnQuotaValidator()
    {
        return new SeasonalAssetReturnQuotaValidator(
            new List<ISeasonalAssetReturnQuotaRule>
            {
                new SeasonalAssetReturnQuotaRequiredTitleRule("seasonal asset return quota needs this field"),
                new SeasonalAssetReturnQuotaReviewerLengthRule(86400),
                new SeasonalAssetReturnQuotaCodePrefixRule("ACM"),
                new SeasonalAssetReturnQuotaRatioRangeRule(2.2, 4988.4),
            });
    }
}
