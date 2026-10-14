namespace Acme.Workforce.Assets.AssetReturn.Agreement;

/// <summary>A rule a asset return digest must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnDigestRule
{
    List<string> Check(AssetReturnDigest item);
}

public sealed record AssetReturnDigestRequiredNotesRule(string Message) : IAssetReturnDigestRule
{
    public List<string> Check(AssetReturnDigest item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnDigestGraceDaysRangeRule(int Min, int Max) : IAssetReturnDigestRule
{
    public List<string> Check(AssetReturnDigest item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssetReturnDigestRequiredLabelRule(string Message) : IAssetReturnDigestRule
{
    public List<string> Check(AssetReturnDigest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnDigestPriorityRangeRule(int Min, int Max) : IAssetReturnDigestRule
{
    public List<string> Check(AssetReturnDigest item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a asset return digest.</summary>
public sealed record AssetReturnDigestValidator(List<IAssetReturnDigestRule> Rules)
{
    public List<string> Validate(AssetReturnDigest item)
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

    public void RequireValid(AssetReturnDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnDigest> items)
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

/// <summary>Functions of asset return digest rules.</summary>
public static class AssetReturnDigestRules
{
    public static AssetReturnDigestValidator DefaultAssetReturnDigestValidator()
    {
        return new AssetReturnDigestValidator(
            new List<IAssetReturnDigestRule>
            {
                new AssetReturnDigestRequiredNotesRule("asset return digest needs this field"),
                new AssetReturnDigestGraceDaysRangeRule(80, 120),
                new AssetReturnDigestRequiredLabelRule("asset return digest needs an entry"),
                new AssetReturnDigestPriorityRangeRule(16, 86400),
            });
    }
}
