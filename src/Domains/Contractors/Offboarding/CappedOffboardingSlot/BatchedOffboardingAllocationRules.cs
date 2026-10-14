namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

/// <summary>A rule a batched offboarding allocation must pass. Check returns its problems, or none.</summary>
public interface IBatchedOffboardingAllocationRule
{
    List<string> Check(BatchedOffboardingAllocation item);
}

public sealed record BatchedOffboardingAllocationMarginRangeRule(double Min, double Max) : IBatchedOffboardingAllocationRule
{
    public List<string> Check(BatchedOffboardingAllocation item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedOffboardingAllocationCodeLengthRule(int MaxLength) : IBatchedOffboardingAllocationRule
{
    public List<string> Check(BatchedOffboardingAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedOffboardingAllocationTagsLimitRule(int MaxCount, List<string> Banned) : IBatchedOffboardingAllocationRule
{
    public List<string> Check(BatchedOffboardingAllocation item)
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

public sealed record BatchedOffboardingAllocationRequiredTimezoneRule(string Message) : IBatchedOffboardingAllocationRule
{
    public List<string> Check(BatchedOffboardingAllocation item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedOffboardingAllocationRequiredLabelRule(string Message) : IBatchedOffboardingAllocationRule
{
    public List<string> Check(BatchedOffboardingAllocation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched offboarding allocation.</summary>
public sealed record BatchedOffboardingAllocationValidator(List<IBatchedOffboardingAllocationRule> Rules)
{
    public List<string> Validate(BatchedOffboardingAllocation item)
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

    public void RequireValid(BatchedOffboardingAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedOffboardingAllocation> items)
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

/// <summary>Functions of batched offboarding allocation rules.</summary>
public static class BatchedOffboardingAllocationRules
{
    public static BatchedOffboardingAllocationValidator DefaultBatchedOffboardingAllocationValidator()
    {
        return new BatchedOffboardingAllocationValidator(
            new List<IBatchedOffboardingAllocationRule>
            {
                new BatchedOffboardingAllocationMarginRangeRule(6.2, 4556.6),
                new BatchedOffboardingAllocationCodeLengthRule(86400),
                new BatchedOffboardingAllocationTagsLimitRule(
                    120,
                    new List<string> { "legacy", "lenient", "strict" }),
                new BatchedOffboardingAllocationRequiredTimezoneRule("batched offboarding allocation needs a value"),
                new BatchedOffboardingAllocationRequiredLabelRule("batched offboarding allocation needs a value"),
            });
    }
}
