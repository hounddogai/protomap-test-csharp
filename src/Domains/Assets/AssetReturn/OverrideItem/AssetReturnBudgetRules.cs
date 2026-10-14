namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>A rule a asset return budget must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnBudgetRule
{
    List<string> Check(AssetReturnBudget item);
}

public sealed record AssetReturnBudgetTeamLengthRule(int MaxLength) : IAssetReturnBudgetRule
{
    public List<string> Check(AssetReturnBudget item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBudgetCeilingRangeRule(double Min, double Max) : IAssetReturnBudgetRule
{
    public List<string> Check(AssetReturnBudget item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBudgetActiveGuardRule(double Limit) : IAssetReturnBudgetRule
{
    public List<string> Check(AssetReturnBudget item)
    {
        if (item.Active && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnBudgetRequiredCodeRule(string Message) : IAssetReturnBudgetRule
{
    public List<string> Check(AssetReturnBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return budget.</summary>
public sealed record AssetReturnBudgetValidator(List<IAssetReturnBudgetRule> Rules)
{
    public List<string> Validate(AssetReturnBudget item)
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

    public void RequireValid(AssetReturnBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnBudget> items)
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

/// <summary>Functions of asset return budget rules.</summary>
public static class AssetReturnBudgetRules
{
    public static AssetReturnBudgetValidator DefaultAssetReturnBudgetValidator()
    {
        return new AssetReturnBudgetValidator(
            new List<IAssetReturnBudgetRule>
            {
                new AssetReturnBudgetTeamLengthRule(1000),
                new AssetReturnBudgetCeilingRangeRule(3.9, 2914.6),
                new AssetReturnBudgetActiveGuardRule(8.0),
                new AssetReturnBudgetRequiredCodeRule("asset return budget needs an entry"),
            });
    }
}
