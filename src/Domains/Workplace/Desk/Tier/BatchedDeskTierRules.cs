namespace Acme.Workforce.Workplace.Desk.Tier;

/// <summary>A rule a batched desk tier must pass. Check returns its problems, or none.</summary>
public interface IBatchedDeskTierRule
{
    List<string> Check(BatchedDeskTier item);
}

public sealed record BatchedDeskTierRequiredLabelRule(string Message) : IBatchedDeskTierRule
{
    public List<string> Check(BatchedDeskTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedDeskTierRequiredCodeRule(string Message) : IBatchedDeskTierRule
{
    public List<string> Check(BatchedDeskTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedDeskTierCodeLengthRule(int MaxLength) : IBatchedDeskTierRule
{
    public List<string> Check(BatchedDeskTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedDeskTierCodePrefixRule(string Prefix) : IBatchedDeskTierRule
{
    public List<string> Check(BatchedDeskTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched desk tier.</summary>
public sealed record BatchedDeskTierValidator(List<IBatchedDeskTierRule> Rules)
{
    public List<string> Validate(BatchedDeskTier item)
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

    public void RequireValid(BatchedDeskTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedDeskTier> items)
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

/// <summary>Functions of batched desk tier rules.</summary>
public static class BatchedDeskTierRules
{
    public static BatchedDeskTierValidator DefaultBatchedDeskTierValidator()
    {
        return new BatchedDeskTierValidator(
            new List<IBatchedDeskTierRule>
            {
                new BatchedDeskTierRequiredLabelRule("batched desk tier needs a value"),
                new BatchedDeskTierRequiredCodeRule("batched desk tier needs an entry"),
                new BatchedDeskTierCodeLengthRule(16),
                new BatchedDeskTierCodePrefixRule("OPS"),
            });
    }
}
