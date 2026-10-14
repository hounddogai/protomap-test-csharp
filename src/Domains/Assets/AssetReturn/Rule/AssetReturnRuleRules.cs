namespace Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>A rule a asset return rule must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnRuleRule
{
    List<string> Check(AssetReturnRule item);
}

public sealed record AssetReturnRuleRequiredCodeRule(string Message) : IAssetReturnRuleRule
{
    public List<string> Check(AssetReturnRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnRuleCountryLengthRule(int MaxLength) : IAssetReturnRuleRule
{
    public List<string> Check(AssetReturnRule item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnRuleGraceDaysRangeRule(int Min, int Max) : IAssetReturnRuleRule
{
    public List<string> Check(AssetReturnRule item)
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

/// <summary>Runs every rule of a asset return rule.</summary>
public sealed record AssetReturnRuleValidator(List<IAssetReturnRuleRule> Rules)
{
    public List<string> Validate(AssetReturnRule item)
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

    public void RequireValid(AssetReturnRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnRule> items)
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

/// <summary>Functions of asset return rule rules.</summary>
public static class AssetReturnRuleRules
{
    public static AssetReturnRuleValidator DefaultAssetReturnRuleValidator()
    {
        return new AssetReturnRuleValidator(
            new List<IAssetReturnRuleRule>
            {
                new AssetReturnRuleRequiredCodeRule("asset return rule needs this field"),
                new AssetReturnRuleCountryLengthRule(80),
                new AssetReturnRuleGraceDaysRangeRule(16, 1000),
            });
    }
}
