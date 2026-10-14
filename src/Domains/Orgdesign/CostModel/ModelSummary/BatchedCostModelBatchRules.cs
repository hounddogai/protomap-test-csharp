namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>A rule a batched cost model batch must pass. Check returns its problems, or none.</summary>
public interface IBatchedCostModelBatchRule
{
    List<string> Check(BatchedCostModelBatch item);
}

public sealed record BatchedCostModelBatchCapacityRangeRule(int Min, int Max) : IBatchedCostModelBatchRule
{
    public List<string> Check(BatchedCostModelBatch item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedCostModelBatchEndsAtRangeRule(int Min, int Max) : IBatchedCostModelBatchRule
{
    public List<string> Check(BatchedCostModelBatch item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedCostModelBatchDepartmentLengthRule(int MaxLength) : IBatchedCostModelBatchRule
{
    public List<string> Check(BatchedCostModelBatch item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedCostModelBatchRequiredTitleRule(string Message) : IBatchedCostModelBatchRule
{
    public List<string> Check(BatchedCostModelBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedCostModelBatchVisibleGuardRule(double Limit) : IBatchedCostModelBatchRule
{
    public List<string> Check(BatchedCostModelBatch item)
    {
        if (item.Visible && item.Budget > Limit)
        {
            return new List<string> { $"budget {item.Budget} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched cost model batch.</summary>
public sealed record BatchedCostModelBatchValidator(List<IBatchedCostModelBatchRule> Rules)
{
    public List<string> Validate(BatchedCostModelBatch item)
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

    public void RequireValid(BatchedCostModelBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedCostModelBatch> items)
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

/// <summary>Functions of batched cost model batch rules.</summary>
public static class BatchedCostModelBatchRules
{
    public static BatchedCostModelBatchValidator DefaultBatchedCostModelBatchValidator()
    {
        return new BatchedCostModelBatchValidator(
            new List<IBatchedCostModelBatchRule>
            {
                new BatchedCostModelBatchCapacityRangeRule(64, 255),
                new BatchedCostModelBatchEndsAtRangeRule(120, 255),
                new BatchedCostModelBatchDepartmentLengthRule(80),
                new BatchedCostModelBatchRequiredTitleRule("batched cost model batch needs this field"),
                new BatchedCostModelBatchVisibleGuardRule(4.2),
            });
    }
}
