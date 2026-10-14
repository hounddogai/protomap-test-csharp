namespace Acme.Workforce.Assets.Laptop.Rollup;

/// <summary>A rule a batched laptop template must pass. Check returns its problems, or none.</summary>
public interface IBatchedLaptopTemplateRule
{
    List<string> Check(BatchedLaptopTemplate item);
}

public sealed record BatchedLaptopTemplateRequiredTitleRule(string Message) : IBatchedLaptopTemplateRule
{
    public List<string> Check(BatchedLaptopTemplate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedLaptopTemplateCodePrefixRule(string Prefix) : IBatchedLaptopTemplateRule
{
    public List<string> Check(BatchedLaptopTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLaptopTemplateCapacityRangeRule(int Min, int Max) : IBatchedLaptopTemplateRule
{
    public List<string> Check(BatchedLaptopTemplate item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedLaptopTemplatePriorityRangeRule(int Min, int Max) : IBatchedLaptopTemplateRule
{
    public List<string> Check(BatchedLaptopTemplate item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLaptopTemplateRequiredCodeRule(string Message) : IBatchedLaptopTemplateRule
{
    public List<string> Check(BatchedLaptopTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedLaptopTemplateTitleLengthRule(int MaxLength) : IBatchedLaptopTemplateRule
{
    public List<string> Check(BatchedLaptopTemplate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched laptop template.</summary>
public sealed record BatchedLaptopTemplateValidator(List<IBatchedLaptopTemplateRule> Rules)
{
    public List<string> Validate(BatchedLaptopTemplate item)
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

    public void RequireValid(BatchedLaptopTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedLaptopTemplate> items)
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

/// <summary>Functions of batched laptop template rules.</summary>
public static class BatchedLaptopTemplateRules
{
    public static BatchedLaptopTemplateValidator DefaultBatchedLaptopTemplateValidator()
    {
        return new BatchedLaptopTemplateValidator(
            new List<IBatchedLaptopTemplateRule>
            {
                new BatchedLaptopTemplateRequiredTitleRule("batched laptop template needs this field"),
                new BatchedLaptopTemplateCodePrefixRule("X"),
                new BatchedLaptopTemplateCapacityRangeRule(80, 86400),
                new BatchedLaptopTemplatePriorityRangeRule(80, 86400),
                new BatchedLaptopTemplateRequiredCodeRule("batched laptop template needs an entry"),
                new BatchedLaptopTemplateTitleLengthRule(120),
            });
    }
}
