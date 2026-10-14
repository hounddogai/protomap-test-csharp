namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

/// <summary>A rule a asset return quota must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnQuotaRule
{
    List<string> Check(AssetReturnQuota item);
}

public sealed record AssetReturnQuotaNameLengthRule(int MaxLength) : IAssetReturnQuotaRule
{
    public List<string> Check(AssetReturnQuota item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnQuotaRequiredTitleRule(string Message) : IAssetReturnQuotaRule
{
    public List<string> Check(AssetReturnQuota item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnQuotaRequiredNameRule(string Message) : IAssetReturnQuotaRule
{
    public List<string> Check(AssetReturnQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnQuotaTitleLengthRule(int MaxLength) : IAssetReturnQuotaRule
{
    public List<string> Check(AssetReturnQuota item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnQuotaRequiredContactEmailRule(string Message) : IAssetReturnQuotaRule
{
    public List<string> Check(AssetReturnQuota item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnQuotaMaximumRangeRule(double Min, double Max) : IAssetReturnQuotaRule
{
    public List<string> Check(AssetReturnQuota item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return quota.</summary>
public sealed record AssetReturnQuotaValidator(List<IAssetReturnQuotaRule> Rules)
{
    public List<string> Validate(AssetReturnQuota item)
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

    public void RequireValid(AssetReturnQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnQuota> items)
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

/// <summary>Functions of asset return quota rules.</summary>
public static class AssetReturnQuotaRules
{
    public static AssetReturnQuotaValidator DefaultAssetReturnQuotaValidator()
    {
        return new AssetReturnQuotaValidator(
            new List<IAssetReturnQuotaRule>
            {
                new AssetReturnQuotaNameLengthRule(32),
                new AssetReturnQuotaRequiredTitleRule("asset return quota needs an entry"),
                new AssetReturnQuotaRequiredNameRule("asset return quota needs a value"),
                new AssetReturnQuotaTitleLengthRule(32),
                new AssetReturnQuotaRequiredContactEmailRule("asset return quota needs this field"),
                new AssetReturnQuotaMaximumRangeRule(2.3, 3056.6),
            });
    }
}
