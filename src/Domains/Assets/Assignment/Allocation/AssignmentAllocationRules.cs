namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a assignment allocation must pass. Check returns its problems, or none.</summary>
public interface IAssignmentAllocationRule
{
    List<string> Check(AssignmentAllocation item);
}

public sealed record AssignmentAllocationCostCenterLengthRule(int MaxLength) : IAssignmentAllocationRule
{
    public List<string> Check(AssignmentAllocation item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentAllocationTagsLimitRule(int MaxCount, List<string> Banned) : IAssignmentAllocationRule
{
    public List<string> Check(AssignmentAllocation item)
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

public sealed record AssignmentAllocationPriorityRangeRule(int Min, int Max) : IAssignmentAllocationRule
{
    public List<string> Check(AssignmentAllocation item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentAllocationAllowedLabelRule(List<string> Allowed) : IAssignmentAllocationRule
{
    public List<string> Check(AssignmentAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentAllocationBatchSizeRangeRule(int Min, int Max) : IAssignmentAllocationRule
{
    public List<string> Check(AssignmentAllocation item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssignmentAllocationRequiredCodeRule(string Message) : IAssignmentAllocationRule
{
    public List<string> Check(AssignmentAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment allocation.</summary>
public sealed record AssignmentAllocationValidator(List<IAssignmentAllocationRule> Rules)
{
    public List<string> Validate(AssignmentAllocation item)
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

    public void RequireValid(AssignmentAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentAllocation> items)
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

/// <summary>Functions of assignment allocation rules.</summary>
public static class AssignmentAllocationRules
{
    public static AssignmentAllocationValidator DefaultAssignmentAllocationValidator()
    {
        return new AssignmentAllocationValidator(
            new List<IAssignmentAllocationRule>
            {
                new AssignmentAllocationCostCenterLengthRule(64),
                new AssignmentAllocationTagsLimitRule(32, new List<string>()),
                new AssignmentAllocationPriorityRangeRule(32, 1000),
                new AssignmentAllocationAllowedLabelRule(new List<string> { "strict" }),
                new AssignmentAllocationBatchSizeRangeRule(64, 3600),
                new AssignmentAllocationRequiredCodeRule("assignment allocation needs a value"),
            });
    }
}
