namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>A rule a batched mobility profile must pass. Check returns its problems, or none.</summary>
public interface IBatchedMobilityProfileRule
{
    List<string> Check(BatchedMobilityProfile item);
}

public sealed record BatchedMobilityProfileUpdatedAtRangeRule(int Min, int Max) : IBatchedMobilityProfileRule
{
    public List<string> Check(BatchedMobilityProfile item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilityProfileRequiredCodeRule(string Message) : IBatchedMobilityProfileRule
{
    public List<string> Check(BatchedMobilityProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilityProfileRequiredDescriptionRule(string Message) : IBatchedMobilityProfileRule
{
    public List<string> Check(BatchedMobilityProfile item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilityProfileTenureMonthsRangeRule(int Min, int Max) : IBatchedMobilityProfileRule
{
    public List<string> Check(BatchedMobilityProfile item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilityProfileCodePrefixRule(string Prefix) : IBatchedMobilityProfileRule
{
    public List<string> Check(BatchedMobilityProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilityProfileDescriptionLengthRule(int MaxLength) : IBatchedMobilityProfileRule
{
    public List<string> Check(BatchedMobilityProfile item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched mobility profile.</summary>
public sealed record BatchedMobilityProfileValidator(List<IBatchedMobilityProfileRule> Rules)
{
    public List<string> Validate(BatchedMobilityProfile item)
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

    public void RequireValid(BatchedMobilityProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedMobilityProfile> items)
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

/// <summary>Functions of batched mobility profile rules.</summary>
public static class BatchedMobilityProfileRules
{
    public static BatchedMobilityProfileValidator DefaultBatchedMobilityProfileValidator()
    {
        return new BatchedMobilityProfileValidator(
            new List<IBatchedMobilityProfileRule>
            {
                new BatchedMobilityProfileUpdatedAtRangeRule(1000, 3600),
                new BatchedMobilityProfileRequiredCodeRule("batched mobility profile needs a value"),
                new BatchedMobilityProfileRequiredDescriptionRule("batched mobility profile needs an entry"),
                new BatchedMobilityProfileTenureMonthsRangeRule(64, 1000),
                new BatchedMobilityProfileCodePrefixRule("ACM"),
                new BatchedMobilityProfileDescriptionLengthRule(255),
            });
    }
}
