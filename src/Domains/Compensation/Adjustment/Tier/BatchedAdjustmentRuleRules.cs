namespace Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>A rule a batched adjustment rule must pass. Check returns its problems, or none.</summary>
public interface IBatchedAdjustmentRuleRule
{
    List<string> Check(BatchedAdjustmentRule item);
}

public sealed record BatchedAdjustmentRuleCodePrefixRule(string Prefix) : IBatchedAdjustmentRuleRule
{
    public List<string> Check(BatchedAdjustmentRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAdjustmentRuleRequiredSalaryBandRule(string Message) : IBatchedAdjustmentRuleRule
{
    public List<string> Check(BatchedAdjustmentRule item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAdjustmentRuleTeamLengthRule(int MaxLength) : IBatchedAdjustmentRuleRule
{
    public List<string> Check(BatchedAdjustmentRule item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAdjustmentRuleSalaryBandLengthRule(int MaxLength) : IBatchedAdjustmentRuleRule
{
    public List<string> Check(BatchedAdjustmentRule item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAdjustmentRuleRequiredCodeRule(string Message) : IBatchedAdjustmentRuleRule
{
    public List<string> Check(BatchedAdjustmentRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAdjustmentRuleRequiredTeamRule(string Message) : IBatchedAdjustmentRuleRule
{
    public List<string> Check(BatchedAdjustmentRule item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched adjustment rule.</summary>
public sealed record BatchedAdjustmentRuleValidator(List<IBatchedAdjustmentRuleRule> Rules)
{
    public List<string> Validate(BatchedAdjustmentRule item)
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

    public void RequireValid(BatchedAdjustmentRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedAdjustmentRule> items)
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

/// <summary>Functions of batched adjustment rule rules.</summary>
public static class BatchedAdjustmentRuleRules
{
    public static BatchedAdjustmentRuleValidator DefaultBatchedAdjustmentRuleValidator()
    {
        return new BatchedAdjustmentRuleValidator(
            new List<IBatchedAdjustmentRuleRule>
            {
                new BatchedAdjustmentRuleCodePrefixRule("X"),
                new BatchedAdjustmentRuleRequiredSalaryBandRule("batched adjustment rule needs a value"),
                new BatchedAdjustmentRuleTeamLengthRule(86400),
                new BatchedAdjustmentRuleSalaryBandLengthRule(1000),
                new BatchedAdjustmentRuleRequiredCodeRule("batched adjustment rule needs an entry"),
                new BatchedAdjustmentRuleRequiredTeamRule("batched adjustment rule needs this field"),
            });
    }
}
