namespace Acme.Workforce.Compliance.Exception.Rule;

/// <summary>A rule a batched exception record must pass. Check returns its problems, or none.</summary>
public interface IBatchedExceptionRecordRule
{
    List<string> Check(BatchedExceptionRecord item);
}

public sealed record BatchedExceptionRecordRequiredCostCenterRule(string Message) : IBatchedExceptionRecordRule
{
    public List<string> Check(BatchedExceptionRecord item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedExceptionRecordMarginRangeRule(double Min, double Max) : IBatchedExceptionRecordRule
{
    public List<string> Check(BatchedExceptionRecord item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedExceptionRecordNameLengthRule(int MaxLength) : IBatchedExceptionRecordRule
{
    public List<string> Check(BatchedExceptionRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedExceptionRecordRequiredCategoryRule(string Message) : IBatchedExceptionRecordRule
{
    public List<string> Check(BatchedExceptionRecord item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedExceptionRecordCodeLengthRule(int MaxLength) : IBatchedExceptionRecordRule
{
    public List<string> Check(BatchedExceptionRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedExceptionRecordCostCenterLengthRule(int MaxLength) : IBatchedExceptionRecordRule
{
    public List<string> Check(BatchedExceptionRecord item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched exception record.</summary>
public sealed record BatchedExceptionRecordValidator(List<IBatchedExceptionRecordRule> Rules)
{
    public List<string> Validate(BatchedExceptionRecord item)
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

    public void RequireValid(BatchedExceptionRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedExceptionRecord> items)
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

/// <summary>Functions of batched exception record rules.</summary>
public static class BatchedExceptionRecordRules
{
    public static BatchedExceptionRecordValidator DefaultBatchedExceptionRecordValidator()
    {
        return new BatchedExceptionRecordValidator(
            new List<IBatchedExceptionRecordRule>
            {
                new BatchedExceptionRecordRequiredCostCenterRule("batched exception record needs this field"),
                new BatchedExceptionRecordMarginRangeRule(8.3, 4181.1),
                new BatchedExceptionRecordNameLengthRule(1000),
                new BatchedExceptionRecordRequiredCategoryRule("batched exception record needs this field"),
                new BatchedExceptionRecordCodeLengthRule(64),
                new BatchedExceptionRecordCostCenterLengthRule(1000),
            });
    }
}
