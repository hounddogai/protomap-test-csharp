namespace Acme.Workforce.Absence.Balance.Ledger;

/// <summary>A rule a batched balance rule must pass. Check returns its problems, or none.</summary>
public interface IBatchedBalanceRuleRule
{
    List<string> Check(BatchedBalanceRule item);
}

public sealed record BatchedBalanceRuleCodePrefixRule(string Prefix) : IBatchedBalanceRuleRule
{
    public List<string> Check(BatchedBalanceRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBalanceRuleTagsLimitRule(int MaxCount, List<string> Banned) : IBatchedBalanceRuleRule
{
    public List<string> Check(BatchedBalanceRule item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record BatchedBalanceRuleRequiredCostCenterRule(string Message) : IBatchedBalanceRuleRule
{
    public List<string> Check(BatchedBalanceRule item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedBalanceRuleSkillLengthRule(int MaxLength) : IBatchedBalanceRuleRule
{
    public List<string> Check(BatchedBalanceRule item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBalanceRuleCodeLengthRule(int MaxLength) : IBatchedBalanceRuleRule
{
    public List<string> Check(BatchedBalanceRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched balance rule.</summary>
public sealed record BatchedBalanceRuleValidator(List<IBatchedBalanceRuleRule> Rules)
{
    public List<string> Validate(BatchedBalanceRule item)
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

    public void RequireValid(BatchedBalanceRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedBalanceRule> items)
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

/// <summary>Functions of batched balance rule rules.</summary>
public static class BatchedBalanceRuleRules
{
    public static BatchedBalanceRuleValidator DefaultBatchedBalanceRuleValidator()
    {
        return new BatchedBalanceRuleValidator(
            new List<IBatchedBalanceRuleRule>
            {
                new BatchedBalanceRuleCodePrefixRule("X"),
                new BatchedBalanceRuleTagsLimitRule(16, new List<string> { "manual", "pending" }),
                new BatchedBalanceRuleRequiredCostCenterRule("batched balance rule needs this field"),
                new BatchedBalanceRuleSkillLengthRule(255),
                new BatchedBalanceRuleCodeLengthRule(1000),
            });
    }
}
