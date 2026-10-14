namespace Acme.Workforce.Assets.AssetReturn.Score;

/// <summary>A rule a legacy asset return plan must pass. Check returns its problems, or none.</summary>
public interface ILegacyAssetReturnPlanRule
{
    List<string> Check(LegacyAssetReturnPlan item);
}

public sealed record LegacyAssetReturnPlanCodePrefixRule(string Prefix) : ILegacyAssetReturnPlanRule
{
    public List<string> Check(LegacyAssetReturnPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAssetReturnPlanRequiredManagerIdRule(string Message) : ILegacyAssetReturnPlanRule
{
    public List<string> Check(LegacyAssetReturnPlan item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAssetReturnPlanRequiredTitleRule(string Message) : ILegacyAssetReturnPlanRule
{
    public List<string> Check(LegacyAssetReturnPlan item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAssetReturnPlanOwnerIdLengthRule(int MaxLength) : ILegacyAssetReturnPlanRule
{
    public List<string> Check(LegacyAssetReturnPlan item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAssetReturnPlanCodeLengthRule(int MaxLength) : ILegacyAssetReturnPlanRule
{
    public List<string> Check(LegacyAssetReturnPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy asset return plan.</summary>
public sealed record LegacyAssetReturnPlanValidator(List<ILegacyAssetReturnPlanRule> Rules)
{
    public List<string> Validate(LegacyAssetReturnPlan item)
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

    public void RequireValid(LegacyAssetReturnPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyAssetReturnPlan> items)
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

/// <summary>Functions of legacy asset return plan rules.</summary>
public static class LegacyAssetReturnPlanRules
{
    public static LegacyAssetReturnPlanValidator DefaultLegacyAssetReturnPlanValidator()
    {
        return new LegacyAssetReturnPlanValidator(
            new List<ILegacyAssetReturnPlanRule>
            {
                new LegacyAssetReturnPlanCodePrefixRule("OPS"),
                new LegacyAssetReturnPlanRequiredManagerIdRule("legacy asset return plan needs a value"),
                new LegacyAssetReturnPlanRequiredTitleRule("legacy asset return plan needs a value"),
                new LegacyAssetReturnPlanOwnerIdLengthRule(80),
                new LegacyAssetReturnPlanCodeLengthRule(120),
            });
    }
}
