namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched access badge target must pass. Check returns its problems, or none.</summary>
public interface IBatchedAccessBadgeTargetRule
{
    List<string> Check(BatchedAccessBadgeTarget item);
}

public sealed record BatchedAccessBadgeTargetAllowedTitleRule(List<string> Allowed) : IBatchedAccessBadgeTargetRule
{
    public List<string> Check(BatchedAccessBadgeTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccessBadgeTargetCodeLengthRule(int MaxLength) : IBatchedAccessBadgeTargetRule
{
    public List<string> Check(BatchedAccessBadgeTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccessBadgeTargetTitleLengthRule(int MaxLength) : IBatchedAccessBadgeTargetRule
{
    public List<string> Check(BatchedAccessBadgeTarget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccessBadgeTargetRequiredCodeRule(string Message) : IBatchedAccessBadgeTargetRule
{
    public List<string> Check(BatchedAccessBadgeTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched access badge target.</summary>
public sealed record BatchedAccessBadgeTargetValidator(List<IBatchedAccessBadgeTargetRule> Rules)
{
    public List<string> Validate(BatchedAccessBadgeTarget item)
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

    public void RequireValid(BatchedAccessBadgeTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedAccessBadgeTarget> items)
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

/// <summary>Functions of batched access badge target rules.</summary>
public static class BatchedAccessBadgeTargetRules
{
    public static BatchedAccessBadgeTargetValidator DefaultBatchedAccessBadgeTargetValidator()
    {
        return new BatchedAccessBadgeTargetValidator(
            new List<IBatchedAccessBadgeTargetRule>
            {
                new BatchedAccessBadgeTargetAllowedTitleRule(new List<string>()),
                new BatchedAccessBadgeTargetCodeLengthRule(255),
                new BatchedAccessBadgeTargetTitleLengthRule(32),
                new BatchedAccessBadgeTargetRequiredCodeRule("batched access badge target needs this field"),
            });
    }
}
