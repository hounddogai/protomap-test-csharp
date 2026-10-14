namespace Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>A rule a batched laptop request must pass. Check returns its problems, or none.</summary>
public interface IBatchedLaptopRequestRule
{
    List<string> Check(BatchedLaptopRequest item);
}

public sealed record BatchedLaptopRequestQuantityRangeRule(int Min, int Max) : IBatchedLaptopRequestRule
{
    public List<string> Check(BatchedLaptopRequest item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedLaptopRequestRequiredLabelRule(string Message) : IBatchedLaptopRequestRule
{
    public List<string> Check(BatchedLaptopRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedLaptopRequestRequiredCodeRule(string Message) : IBatchedLaptopRequestRule
{
    public List<string> Check(BatchedLaptopRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched laptop request.</summary>
public sealed record BatchedLaptopRequestValidator(List<IBatchedLaptopRequestRule> Rules)
{
    public List<string> Validate(BatchedLaptopRequest item)
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

    public void RequireValid(BatchedLaptopRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedLaptopRequest> items)
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

/// <summary>Functions of batched laptop request rules.</summary>
public static class BatchedLaptopRequestRules
{
    public static BatchedLaptopRequestValidator DefaultBatchedLaptopRequestValidator()
    {
        return new BatchedLaptopRequestValidator(
            new List<IBatchedLaptopRequestRule>
            {
                new BatchedLaptopRequestQuantityRangeRule(1000, 3600),
                new BatchedLaptopRequestRequiredLabelRule("batched laptop request needs an entry"),
                new BatchedLaptopRequestRequiredCodeRule("batched laptop request needs this field"),
            });
    }
}
