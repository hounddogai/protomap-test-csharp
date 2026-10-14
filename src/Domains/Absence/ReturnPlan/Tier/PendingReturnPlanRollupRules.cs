namespace Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>A rule a pending return plan rollup must pass. Check returns its problems, or none.</summary>
public interface IPendingReturnPlanRollupRule
{
    List<string> Check(PendingReturnPlanRollup item);
}

public sealed record PendingReturnPlanRollupQuantityRangeRule(int Min, int Max) : IPendingReturnPlanRollupRule
{
    public List<string> Check(PendingReturnPlanRollup item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingReturnPlanRollupWeightRangeRule(double Min, double Max) : IPendingReturnPlanRollupRule
{
    public List<string> Check(PendingReturnPlanRollup item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingReturnPlanRollupCostRangeRule(double Min, double Max) : IPendingReturnPlanRollupRule
{
    public List<string> Check(PendingReturnPlanRollup item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending return plan rollup.</summary>
public sealed record PendingReturnPlanRollupValidator(List<IPendingReturnPlanRollupRule> Rules)
{
    public List<string> Validate(PendingReturnPlanRollup item)
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

    public void RequireValid(PendingReturnPlanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingReturnPlanRollup> items)
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

/// <summary>Functions of pending return plan rollup rules.</summary>
public static class PendingReturnPlanRollupRules
{
    public static PendingReturnPlanRollupValidator DefaultPendingReturnPlanRollupValidator()
    {
        return new PendingReturnPlanRollupValidator(
            new List<IPendingReturnPlanRollupRule>
            {
                new PendingReturnPlanRollupQuantityRangeRule(255, 86400),
                new PendingReturnPlanRollupWeightRangeRule(7.1, 1170.7),
                new PendingReturnPlanRollupCostRangeRule(7.6, 1753.0),
            });
    }
}
