namespace Acme.Workforce.Talent.Mobility.Summary;

/// <summary>A rule a batched mobility manifest must pass. Check returns its problems, or none.</summary>
public interface IBatchedMobilityManifestRule
{
    List<string> Check(BatchedMobilityManifest item);
}

public sealed record BatchedMobilityManifestSalaryBandLengthRule(int MaxLength) : IBatchedMobilityManifestRule
{
    public List<string> Check(BatchedMobilityManifest item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilityManifestRetriesRangeRule(int Min, int Max) : IBatchedMobilityManifestRule
{
    public List<string> Check(BatchedMobilityManifest item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedMobilityManifestLabelLengthRule(int MaxLength) : IBatchedMobilityManifestRule
{
    public List<string> Check(BatchedMobilityManifest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilityManifestCodePrefixRule(string Prefix) : IBatchedMobilityManifestRule
{
    public List<string> Check(BatchedMobilityManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilityManifestVisibleGuardRule(int Limit) : IBatchedMobilityManifestRule
{
    public List<string> Check(BatchedMobilityManifest item)
    {
        if (item.Visible && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched mobility manifest.</summary>
public sealed record BatchedMobilityManifestValidator(List<IBatchedMobilityManifestRule> Rules)
{
    public List<string> Validate(BatchedMobilityManifest item)
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

    public void RequireValid(BatchedMobilityManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedMobilityManifest> items)
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

/// <summary>Functions of batched mobility manifest rules.</summary>
public static class BatchedMobilityManifestRules
{
    public static BatchedMobilityManifestValidator DefaultBatchedMobilityManifestValidator()
    {
        return new BatchedMobilityManifestValidator(
            new List<IBatchedMobilityManifestRule>
            {
                new BatchedMobilityManifestSalaryBandLengthRule(86400),
                new BatchedMobilityManifestRetriesRangeRule(80, 255),
                new BatchedMobilityManifestLabelLengthRule(120),
                new BatchedMobilityManifestCodePrefixRule("Q"),
                new BatchedMobilityManifestVisibleGuardRule(120),
            });
    }
}
