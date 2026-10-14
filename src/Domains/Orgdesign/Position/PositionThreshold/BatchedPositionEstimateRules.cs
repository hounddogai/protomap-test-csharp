namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>A rule a batched position estimate must pass. Check returns its problems, or none.</summary>
public interface IBatchedPositionEstimateRule
{
    List<string> Check(BatchedPositionEstimate item);
}

public sealed record BatchedPositionEstimateLocationLengthRule(int MaxLength) : IBatchedPositionEstimateRule
{
    public List<string> Check(BatchedPositionEstimate item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPositionEstimateCodePrefixRule(string Prefix) : IBatchedPositionEstimateRule
{
    public List<string> Check(BatchedPositionEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPositionEstimateNameLengthRule(int MaxLength) : IBatchedPositionEstimateRule
{
    public List<string> Check(BatchedPositionEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPositionEstimateCodeLengthRule(int MaxLength) : IBatchedPositionEstimateRule
{
    public List<string> Check(BatchedPositionEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPositionEstimateSummaryLengthRule(int MaxLength) : IBatchedPositionEstimateRule
{
    public List<string> Check(BatchedPositionEstimate item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPositionEstimateRequiredLocationRule(string Message) : IBatchedPositionEstimateRule
{
    public List<string> Check(BatchedPositionEstimate item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched position estimate.</summary>
public sealed record BatchedPositionEstimateValidator(List<IBatchedPositionEstimateRule> Rules)
{
    public List<string> Validate(BatchedPositionEstimate item)
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

    public void RequireValid(BatchedPositionEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedPositionEstimate> items)
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

/// <summary>Functions of batched position estimate rules.</summary>
public static class BatchedPositionEstimateRules
{
    public static BatchedPositionEstimateValidator DefaultBatchedPositionEstimateValidator()
    {
        return new BatchedPositionEstimateValidator(
            new List<IBatchedPositionEstimateRule>
            {
                new BatchedPositionEstimateLocationLengthRule(3600),
                new BatchedPositionEstimateCodePrefixRule("REF"),
                new BatchedPositionEstimateNameLengthRule(80),
                new BatchedPositionEstimateCodeLengthRule(255),
                new BatchedPositionEstimateSummaryLengthRule(3600),
                new BatchedPositionEstimateRequiredLocationRule("batched position estimate needs a value"),
            });
    }
}
