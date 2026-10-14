namespace Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>A rule a asset return notice must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnNoticeRule
{
    List<string> Check(AssetReturnNotice item);
}

public sealed record AssetReturnNoticeFloorValueRangeRule(double Min, double Max) : IAssetReturnNoticeRule
{
    public List<string> Check(AssetReturnNotice item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnNoticeContactEmailLengthRule(int MaxLength) : IAssetReturnNoticeRule
{
    public List<string> Check(AssetReturnNotice item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnNoticeRequiredCategoryRule(string Message) : IAssetReturnNoticeRule
{
    public List<string> Check(AssetReturnNotice item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return notice.</summary>
public sealed record AssetReturnNoticeValidator(List<IAssetReturnNoticeRule> Rules)
{
    public List<string> Validate(AssetReturnNotice item)
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

    public void RequireValid(AssetReturnNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnNotice> items)
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

/// <summary>Functions of asset return notice rules.</summary>
public static class AssetReturnNoticeRules
{
    public static AssetReturnNoticeValidator DefaultAssetReturnNoticeValidator()
    {
        return new AssetReturnNoticeValidator(
            new List<IAssetReturnNoticeRule>
            {
                new AssetReturnNoticeFloorValueRangeRule(1.8, 375.5),
                new AssetReturnNoticeContactEmailLengthRule(32),
                new AssetReturnNoticeRequiredCategoryRule("asset return notice needs this field"),
            });
    }
}
