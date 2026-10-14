namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

/// <summary>A rule a regional succession plan adjustment must pass. Check returns its problems, or none.</summary>
public interface IRegionalSuccessionPlanAdjustmentRule
{
    List<string> Check(RegionalSuccessionPlanAdjustment item);
}

public sealed record RegionalSuccessionPlanAdjustmentCodePrefixRule(string Prefix) : IRegionalSuccessionPlanAdjustmentRule
{
    public List<string> Check(RegionalSuccessionPlanAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanAdjustmentBatchSizeRangeRule(int Min, int Max) : IRegionalSuccessionPlanAdjustmentRule
{
    public List<string> Check(RegionalSuccessionPlanAdjustment item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanAdjustmentAmountRangeRule(double Min, double Max) : IRegionalSuccessionPlanAdjustmentRule
{
    public List<string> Check(RegionalSuccessionPlanAdjustment item)
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

/// <summary>Runs every rule of a regional succession plan adjustment.</summary>
public sealed record RegionalSuccessionPlanAdjustmentValidator(List<IRegionalSuccessionPlanAdjustmentRule> Rules)
{
    public List<string> Validate(RegionalSuccessionPlanAdjustment item)
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

    public void RequireValid(RegionalSuccessionPlanAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSuccessionPlanAdjustment> items)
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

/// <summary>Functions of regional succession plan adjustment rules.</summary>
public static class RegionalSuccessionPlanAdjustmentRules
{
    public static RegionalSuccessionPlanAdjustmentValidator DefaultRegionalSuccessionPlanAdjustmentValidator()
    {
        return new RegionalSuccessionPlanAdjustmentValidator(
            new List<IRegionalSuccessionPlanAdjustmentRule>
            {
                new RegionalSuccessionPlanAdjustmentCodePrefixRule("X"),
                new RegionalSuccessionPlanAdjustmentBatchSizeRangeRule(32, 86400),
                new RegionalSuccessionPlanAdjustmentAmountRangeRule(2.1, 3900.9),
            });
    }
}
