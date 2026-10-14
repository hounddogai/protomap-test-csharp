namespace Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>A rule a batched adjustment request must pass. Check returns its problems, or none.</summary>
public interface IBatchedAdjustmentRequestRule
{
    List<string> Check(BatchedAdjustmentRequest item);
}

public sealed record BatchedAdjustmentRequestSkillLengthRule(int MaxLength) : IBatchedAdjustmentRequestRule
{
    public List<string> Check(BatchedAdjustmentRequest item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAdjustmentRequestPinnedGuardRule(int Limit) : IBatchedAdjustmentRequestRule
{
    public List<string> Check(BatchedAdjustmentRequest item)
    {
        if (item.Pinned && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAdjustmentRequestRequiredBadgeRule(string Message) : IBatchedAdjustmentRequestRule
{
    public List<string> Check(BatchedAdjustmentRequest item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAdjustmentRequestRequiredSkillRule(string Message) : IBatchedAdjustmentRequestRule
{
    public List<string> Check(BatchedAdjustmentRequest item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAdjustmentRequestBadgeLengthRule(int MaxLength) : IBatchedAdjustmentRequestRule
{
    public List<string> Check(BatchedAdjustmentRequest item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched adjustment request.</summary>
public sealed record BatchedAdjustmentRequestValidator(List<IBatchedAdjustmentRequestRule> Rules)
{
    public List<string> Validate(BatchedAdjustmentRequest item)
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

    public void RequireValid(BatchedAdjustmentRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedAdjustmentRequest> items)
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

/// <summary>Functions of batched adjustment request rules.</summary>
public static class BatchedAdjustmentRequestRules
{
    public static BatchedAdjustmentRequestValidator DefaultBatchedAdjustmentRequestValidator()
    {
        return new BatchedAdjustmentRequestValidator(
            new List<IBatchedAdjustmentRequestRule>
            {
                new BatchedAdjustmentRequestSkillLengthRule(1000),
                new BatchedAdjustmentRequestPinnedGuardRule(16),
                new BatchedAdjustmentRequestRequiredBadgeRule("batched adjustment request needs this field"),
                new BatchedAdjustmentRequestRequiredSkillRule("batched adjustment request needs an entry"),
                new BatchedAdjustmentRequestBadgeLengthRule(86400),
            });
    }
}
