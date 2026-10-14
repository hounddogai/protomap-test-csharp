namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered bonus pool batch must pass. Check returns its problems, or none.</summary>
public interface ITieredBonusPoolBatchRule
{
    List<string> Check(TieredBonusPoolBatch item);
}

public sealed record TieredBonusPoolBatchRequiredCostCenterRule(string Message) : ITieredBonusPoolBatchRule
{
    public List<string> Check(TieredBonusPoolBatch item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredBonusPoolBatchCodeLengthRule(int MaxLength) : ITieredBonusPoolBatchRule
{
    public List<string> Check(TieredBonusPoolBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBonusPoolBatchNameLengthRule(int MaxLength) : ITieredBonusPoolBatchRule
{
    public List<string> Check(TieredBonusPoolBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBonusPoolBatchRequiredNameRule(string Message) : ITieredBonusPoolBatchRule
{
    public List<string> Check(TieredBonusPoolBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredBonusPoolBatchAllowedCostCenterRule(List<string> Allowed) : ITieredBonusPoolBatchRule
{
    public List<string> Check(TieredBonusPoolBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered bonus pool batch.</summary>
public sealed record TieredBonusPoolBatchValidator(List<ITieredBonusPoolBatchRule> Rules)
{
    public List<string> Validate(TieredBonusPoolBatch item)
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

    public void RequireValid(TieredBonusPoolBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredBonusPoolBatch> items)
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

/// <summary>Functions of tiered bonus pool batch rules.</summary>
public static class TieredBonusPoolBatchRules
{
    public static TieredBonusPoolBatchValidator DefaultTieredBonusPoolBatchValidator()
    {
        return new TieredBonusPoolBatchValidator(
            new List<ITieredBonusPoolBatchRule>
            {
                new TieredBonusPoolBatchRequiredCostCenterRule("tiered bonus pool batch needs a value"),
                new TieredBonusPoolBatchCodeLengthRule(64),
                new TieredBonusPoolBatchNameLengthRule(255),
                new TieredBonusPoolBatchRequiredNameRule("tiered bonus pool batch needs an entry"),
                new TieredBonusPoolBatchAllowedCostCenterRule(new List<string> { "primary", "primary", "primary" }),
            });
    }
}
