namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a asset return plan must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnPlanRule
{
    List<string> Check(AssetReturnPlan item);
}

public sealed record AssetReturnPlanRequiredReviewerRule(string Message) : IAssetReturnPlanRule
{
    public List<string> Check(AssetReturnPlan item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPlanReviewerLengthRule(int MaxLength) : IAssetReturnPlanRule
{
    public List<string> Check(AssetReturnPlan item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPlanRequiredNotesRule(string Message) : IAssetReturnPlanRule
{
    public List<string> Check(AssetReturnPlan item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPlanRequiredTitleRule(string Message) : IAssetReturnPlanRule
{
    public List<string> Check(AssetReturnPlan item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPlanAllowedCodeRule(List<string> Allowed) : IAssetReturnPlanRule
{
    public List<string> Check(AssetReturnPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPlanTitleLengthRule(int MaxLength) : IAssetReturnPlanRule
{
    public List<string> Check(AssetReturnPlan item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return plan.</summary>
public sealed record AssetReturnPlanValidator(List<IAssetReturnPlanRule> Rules)
{
    public List<string> Validate(AssetReturnPlan item)
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

    public void RequireValid(AssetReturnPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnPlan> items)
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

/// <summary>Functions of asset return plan rules.</summary>
public static class AssetReturnPlanRules
{
    public static AssetReturnPlanValidator DefaultAssetReturnPlanValidator()
    {
        return new AssetReturnPlanValidator(
            new List<IAssetReturnPlanRule>
            {
                new AssetReturnPlanRequiredReviewerRule("asset return plan needs this field"),
                new AssetReturnPlanReviewerLengthRule(80),
                new AssetReturnPlanRequiredNotesRule("asset return plan needs this field"),
                new AssetReturnPlanRequiredTitleRule("asset return plan needs an entry"),
                new AssetReturnPlanAllowedCodeRule(new List<string> { "lenient" }),
                new AssetReturnPlanTitleLengthRule(120),
            });
    }
}
