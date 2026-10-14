namespace Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>A rule a asset return draft must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnDraftRule
{
    List<string> Check(AssetReturnDraft item);
}

public sealed record AssetReturnDraftRequiredCodeRule(string Message) : IAssetReturnDraftRule
{
    public List<string> Check(AssetReturnDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnDraftDiscountRangeRule(double Min, double Max) : IAssetReturnDraftRule
{
    public List<string> Check(AssetReturnDraft item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssetReturnDraftLimitRangeRule(int Min, int Max) : IAssetReturnDraftRule
{
    public List<string> Check(AssetReturnDraft item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return draft.</summary>
public sealed record AssetReturnDraftValidator(List<IAssetReturnDraftRule> Rules)
{
    public List<string> Validate(AssetReturnDraft item)
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

    public void RequireValid(AssetReturnDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnDraft> items)
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

/// <summary>Functions of asset return draft rules.</summary>
public static class AssetReturnDraftRules
{
    public static AssetReturnDraftValidator DefaultAssetReturnDraftValidator()
    {
        return new AssetReturnDraftValidator(
            new List<IAssetReturnDraftRule>
            {
                new AssetReturnDraftRequiredCodeRule("asset return draft needs this field"),
                new AssetReturnDraftDiscountRangeRule(7.3, 630.1),
                new AssetReturnDraftLimitRangeRule(32, 1000),
            });
    }
}
