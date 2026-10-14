namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a asset return segment must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnSegmentRule
{
    List<string> Check(AssetReturnSegment item);
}

public sealed record AssetReturnSegmentCodeLengthRule(int MaxLength) : IAssetReturnSegmentRule
{
    public List<string> Check(AssetReturnSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnSegmentCategoryLengthRule(int MaxLength) : IAssetReturnSegmentRule
{
    public List<string> Check(AssetReturnSegment item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnSegmentRequiredCodeRule(string Message) : IAssetReturnSegmentRule
{
    public List<string> Check(AssetReturnSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnSegmentAllowedGradeRule(List<string> Allowed) : IAssetReturnSegmentRule
{
    public List<string> Check(AssetReturnSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return segment.</summary>
public sealed record AssetReturnSegmentValidator(List<IAssetReturnSegmentRule> Rules)
{
    public List<string> Validate(AssetReturnSegment item)
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

    public void RequireValid(AssetReturnSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnSegment> items)
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

/// <summary>Functions of asset return segment rules.</summary>
public static class AssetReturnSegmentRules
{
    public static AssetReturnSegmentValidator DefaultAssetReturnSegmentValidator()
    {
        return new AssetReturnSegmentValidator(
            new List<IAssetReturnSegmentRule>
            {
                new AssetReturnSegmentCodeLengthRule(32),
                new AssetReturnSegmentCategoryLengthRule(16),
                new AssetReturnSegmentRequiredCodeRule("asset return segment needs a value"),
                new AssetReturnSegmentAllowedGradeRule(new List<string> { "weighted" }),
            });
    }
}
