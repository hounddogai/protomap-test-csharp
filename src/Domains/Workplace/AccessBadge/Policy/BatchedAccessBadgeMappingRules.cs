namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched access badge mapping must pass. Check returns its problems, or none.</summary>
public interface IBatchedAccessBadgeMappingRule
{
    List<string> Check(BatchedAccessBadgeMapping item);
}

public sealed record BatchedAccessBadgeMappingAllowedCategoryRule(List<string> Allowed) : IBatchedAccessBadgeMappingRule
{
    public List<string> Check(BatchedAccessBadgeMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccessBadgeMappingRequiredCategoryRule(string Message) : IBatchedAccessBadgeMappingRule
{
    public List<string> Check(BatchedAccessBadgeMapping item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccessBadgeMappingCodeLengthRule(int MaxLength) : IBatchedAccessBadgeMappingRule
{
    public List<string> Check(BatchedAccessBadgeMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched access badge mapping.</summary>
public sealed record BatchedAccessBadgeMappingValidator(List<IBatchedAccessBadgeMappingRule> Rules)
{
    public List<string> Validate(BatchedAccessBadgeMapping item)
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

    public void RequireValid(BatchedAccessBadgeMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedAccessBadgeMapping> items)
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

/// <summary>Functions of batched access badge mapping rules.</summary>
public static class BatchedAccessBadgeMappingRules
{
    public static BatchedAccessBadgeMappingValidator DefaultBatchedAccessBadgeMappingValidator()
    {
        return new BatchedAccessBadgeMappingValidator(
            new List<IBatchedAccessBadgeMappingRule>
            {
                new BatchedAccessBadgeMappingAllowedCategoryRule(new List<string>()),
                new BatchedAccessBadgeMappingRequiredCategoryRule("batched access badge mapping needs an entry"),
                new BatchedAccessBadgeMappingCodeLengthRule(16),
            });
    }
}
