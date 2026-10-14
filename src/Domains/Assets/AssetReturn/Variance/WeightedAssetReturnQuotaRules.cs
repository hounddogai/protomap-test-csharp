namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted asset return quota must pass. Check returns its problems, or none.</summary>
public interface IWeightedAssetReturnQuotaRule
{
    List<string> Check(WeightedAssetReturnQuota item);
}

public sealed record WeightedAssetReturnQuotaRequiredCodeRule(string Message) : IWeightedAssetReturnQuotaRule
{
    public List<string> Check(WeightedAssetReturnQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAssetReturnQuotaRequiredChecksumRule(string Message) : IWeightedAssetReturnQuotaRule
{
    public List<string> Check(WeightedAssetReturnQuota item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAssetReturnQuotaAllowedTitleRule(List<string> Allowed) : IWeightedAssetReturnQuotaRule
{
    public List<string> Check(WeightedAssetReturnQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAssetReturnQuotaRequiredTitleRule(string Message) : IWeightedAssetReturnQuotaRule
{
    public List<string> Check(WeightedAssetReturnQuota item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted asset return quota.</summary>
public sealed record WeightedAssetReturnQuotaValidator(List<IWeightedAssetReturnQuotaRule> Rules)
{
    public List<string> Validate(WeightedAssetReturnQuota item)
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

    public void RequireValid(WeightedAssetReturnQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAssetReturnQuota> items)
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

/// <summary>Functions of weighted asset return quota rules.</summary>
public static class WeightedAssetReturnQuotaRules
{
    public static WeightedAssetReturnQuotaValidator DefaultWeightedAssetReturnQuotaValidator()
    {
        return new WeightedAssetReturnQuotaValidator(
            new List<IWeightedAssetReturnQuotaRule>
            {
                new WeightedAssetReturnQuotaRequiredCodeRule("weighted asset return quota needs a value"),
                new WeightedAssetReturnQuotaRequiredChecksumRule("weighted asset return quota needs this field"),
                new WeightedAssetReturnQuotaAllowedTitleRule(new List<string> { "batched", "primary", "strict" }),
                new WeightedAssetReturnQuotaRequiredTitleRule("weighted asset return quota needs an entry"),
            });
    }
}
