namespace Acme.Workforce.Assets.AssetReturn.Assignment;

/// <summary>A rule a asset return assignment must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnAssignmentRule
{
    List<string> Check(AssetReturnAssignment item);
}

public sealed record AssetReturnAssignmentRequiredTitleRule(string Message) : IAssetReturnAssignmentRule
{
    public List<string> Check(AssetReturnAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAssignmentRequiredCodeRule(string Message) : IAssetReturnAssignmentRule
{
    public List<string> Check(AssetReturnAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAssignmentRequiredLocationRule(string Message) : IAssetReturnAssignmentRule
{
    public List<string> Check(AssetReturnAssignment item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return assignment.</summary>
public sealed record AssetReturnAssignmentValidator(List<IAssetReturnAssignmentRule> Rules)
{
    public List<string> Validate(AssetReturnAssignment item)
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

    public void RequireValid(AssetReturnAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnAssignment> items)
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

/// <summary>Functions of asset return assignment rules.</summary>
public static class AssetReturnAssignmentRules
{
    public static AssetReturnAssignmentValidator DefaultAssetReturnAssignmentValidator()
    {
        return new AssetReturnAssignmentValidator(
            new List<IAssetReturnAssignmentRule>
            {
                new AssetReturnAssignmentRequiredTitleRule("asset return assignment needs an entry"),
                new AssetReturnAssignmentRequiredCodeRule("asset return assignment needs a value"),
                new AssetReturnAssignmentRequiredLocationRule("asset return assignment needs a value"),
            });
    }
}
