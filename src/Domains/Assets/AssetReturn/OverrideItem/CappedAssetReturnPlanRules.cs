namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>A rule a capped asset return plan must pass. Check returns its problems, or none.</summary>
public interface ICappedAssetReturnPlanRule
{
    List<string> Check(CappedAssetReturnPlan item);
}

public sealed record CappedAssetReturnPlanRequiredSegmentRule(string Message) : ICappedAssetReturnPlanRule
{
    public List<string> Check(CappedAssetReturnPlan item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAssetReturnPlanRequiredNameRule(string Message) : ICappedAssetReturnPlanRule
{
    public List<string> Check(CappedAssetReturnPlan item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAssetReturnPlanAmountRangeRule(double Min, double Max) : ICappedAssetReturnPlanRule
{
    public List<string> Check(CappedAssetReturnPlan item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a capped asset return plan.</summary>
public sealed record CappedAssetReturnPlanValidator(List<ICappedAssetReturnPlanRule> Rules)
{
    public List<string> Validate(CappedAssetReturnPlan item)
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

    public void RequireValid(CappedAssetReturnPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAssetReturnPlan> items)
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

/// <summary>Functions of capped asset return plan rules.</summary>
public static class CappedAssetReturnPlanRules
{
    public static CappedAssetReturnPlanValidator DefaultCappedAssetReturnPlanValidator()
    {
        return new CappedAssetReturnPlanValidator(
            new List<ICappedAssetReturnPlanRule>
            {
                new CappedAssetReturnPlanRequiredSegmentRule("capped asset return plan needs this field"),
                new CappedAssetReturnPlanRequiredNameRule("capped asset return plan needs an entry"),
                new CappedAssetReturnPlanAmountRangeRule(9.5, 3130.7),
            });
    }
}
