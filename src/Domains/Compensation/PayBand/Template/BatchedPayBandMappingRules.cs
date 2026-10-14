namespace Acme.Workforce.Compensation.PayBand.Template;

/// <summary>A rule a batched pay band mapping must pass. Check returns its problems, or none.</summary>
public interface IBatchedPayBandMappingRule
{
    List<string> Check(BatchedPayBandMapping item);
}

public sealed record BatchedPayBandMappingNameLengthRule(int MaxLength) : IBatchedPayBandMappingRule
{
    public List<string> Check(BatchedPayBandMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayBandMappingDescriptionLengthRule(int MaxLength) : IBatchedPayBandMappingRule
{
    public List<string> Check(BatchedPayBandMapping item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayBandMappingCodeLengthRule(int MaxLength) : IBatchedPayBandMappingRule
{
    public List<string> Check(BatchedPayBandMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayBandMappingThresholdRangeRule(double Min, double Max) : IBatchedPayBandMappingRule
{
    public List<string> Check(BatchedPayBandMapping item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched pay band mapping.</summary>
public sealed record BatchedPayBandMappingValidator(List<IBatchedPayBandMappingRule> Rules)
{
    public List<string> Validate(BatchedPayBandMapping item)
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

    public void RequireValid(BatchedPayBandMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedPayBandMapping> items)
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

/// <summary>Functions of batched pay band mapping rules.</summary>
public static class BatchedPayBandMappingRules
{
    public static BatchedPayBandMappingValidator DefaultBatchedPayBandMappingValidator()
    {
        return new BatchedPayBandMappingValidator(
            new List<IBatchedPayBandMappingRule>
            {
                new BatchedPayBandMappingNameLengthRule(32),
                new BatchedPayBandMappingDescriptionLengthRule(255),
                new BatchedPayBandMappingCodeLengthRule(32),
                new BatchedPayBandMappingThresholdRangeRule(2.8, 2818.4),
            });
    }
}
