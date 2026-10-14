namespace Acme.Workforce.Assets.AssetReturn.Request;

/// <summary>A rule a asset return request must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnRequestRule
{
    List<string> Check(AssetReturnRequest item);
}

public sealed record AssetReturnRequestTitleLengthRule(int MaxLength) : IAssetReturnRequestRule
{
    public List<string> Check(AssetReturnRequest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnRequestRequiredCodeRule(string Message) : IAssetReturnRequestRule
{
    public List<string> Check(AssetReturnRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnRequestRequiredTitleRule(string Message) : IAssetReturnRequestRule
{
    public List<string> Check(AssetReturnRequest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnRequestRequiredContactEmailRule(string Message) : IAssetReturnRequestRule
{
    public List<string> Check(AssetReturnRequest item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return request.</summary>
public sealed record AssetReturnRequestValidator(List<IAssetReturnRequestRule> Rules)
{
    public List<string> Validate(AssetReturnRequest item)
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

    public void RequireValid(AssetReturnRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnRequest> items)
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

/// <summary>Functions of asset return request rules.</summary>
public static class AssetReturnRequestRules
{
    public static AssetReturnRequestValidator DefaultAssetReturnRequestValidator()
    {
        return new AssetReturnRequestValidator(
            new List<IAssetReturnRequestRule>
            {
                new AssetReturnRequestTitleLengthRule(32),
                new AssetReturnRequestRequiredCodeRule("asset return request needs an entry"),
                new AssetReturnRequestRequiredTitleRule("asset return request needs an entry"),
                new AssetReturnRequestRequiredContactEmailRule("asset return request needs an entry"),
            });
    }
}
