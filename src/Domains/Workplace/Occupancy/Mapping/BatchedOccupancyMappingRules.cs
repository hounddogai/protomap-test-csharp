namespace Acme.Workforce.Workplace.Occupancy.Mapping;

/// <summary>A rule a batched occupancy mapping must pass. Check returns its problems, or none.</summary>
public interface IBatchedOccupancyMappingRule
{
    List<string> Check(BatchedOccupancyMapping item);
}

public sealed record BatchedOccupancyMappingNameLengthRule(int MaxLength) : IBatchedOccupancyMappingRule
{
    public List<string> Check(BatchedOccupancyMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedOccupancyMappingRequiredExternalRefRule(string Message) : IBatchedOccupancyMappingRule
{
    public List<string> Check(BatchedOccupancyMapping item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedOccupancyMappingCodePrefixRule(string Prefix) : IBatchedOccupancyMappingRule
{
    public List<string> Check(BatchedOccupancyMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedOccupancyMappingLimitRangeRule(int Min, int Max) : IBatchedOccupancyMappingRule
{
    public List<string> Check(BatchedOccupancyMapping item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedOccupancyMappingRequiredNameRule(string Message) : IBatchedOccupancyMappingRule
{
    public List<string> Check(BatchedOccupancyMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedOccupancyMappingRequiredReviewerRule(string Message) : IBatchedOccupancyMappingRule
{
    public List<string> Check(BatchedOccupancyMapping item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched occupancy mapping.</summary>
public sealed record BatchedOccupancyMappingValidator(List<IBatchedOccupancyMappingRule> Rules)
{
    public List<string> Validate(BatchedOccupancyMapping item)
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

    public void RequireValid(BatchedOccupancyMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedOccupancyMapping> items)
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

/// <summary>Functions of batched occupancy mapping rules.</summary>
public static class BatchedOccupancyMappingRules
{
    public static BatchedOccupancyMappingValidator DefaultBatchedOccupancyMappingValidator()
    {
        return new BatchedOccupancyMappingValidator(
            new List<IBatchedOccupancyMappingRule>
            {
                new BatchedOccupancyMappingNameLengthRule(64),
                new BatchedOccupancyMappingRequiredExternalRefRule("batched occupancy mapping needs an entry"),
                new BatchedOccupancyMappingCodePrefixRule("ACM"),
                new BatchedOccupancyMappingLimitRangeRule(1000, 1000),
                new BatchedOccupancyMappingRequiredNameRule("batched occupancy mapping needs an entry"),
                new BatchedOccupancyMappingRequiredReviewerRule("batched occupancy mapping needs this field"),
            });
    }
}
