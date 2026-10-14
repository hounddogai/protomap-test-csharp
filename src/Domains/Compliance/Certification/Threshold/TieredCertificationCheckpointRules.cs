namespace Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>A rule a tiered certification checkpoint must pass. Check returns its problems, or none.</summary>
public interface ITieredCertificationCheckpointRule
{
    List<string> Check(TieredCertificationCheckpoint item);
}

public sealed record TieredCertificationCheckpointJobCodeLengthRule(int MaxLength) : ITieredCertificationCheckpointRule
{
    public List<string> Check(TieredCertificationCheckpoint item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredCertificationCheckpointNameLengthRule(int MaxLength) : ITieredCertificationCheckpointRule
{
    public List<string> Check(TieredCertificationCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredCertificationCheckpointCurrencyLengthRule(int MaxLength) : ITieredCertificationCheckpointRule
{
    public List<string> Check(TieredCertificationCheckpoint item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredCertificationCheckpointVersionRangeRule(int Min, int Max) : ITieredCertificationCheckpointRule
{
    public List<string> Check(TieredCertificationCheckpoint item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered certification checkpoint.</summary>
public sealed record TieredCertificationCheckpointValidator(List<ITieredCertificationCheckpointRule> Rules)
{
    public List<string> Validate(TieredCertificationCheckpoint item)
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

    public void RequireValid(TieredCertificationCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredCertificationCheckpoint> items)
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

/// <summary>Functions of tiered certification checkpoint rules.</summary>
public static class TieredCertificationCheckpointRules
{
    public static TieredCertificationCheckpointValidator DefaultTieredCertificationCheckpointValidator()
    {
        return new TieredCertificationCheckpointValidator(
            new List<ITieredCertificationCheckpointRule>
            {
                new TieredCertificationCheckpointJobCodeLengthRule(86400),
                new TieredCertificationCheckpointNameLengthRule(16),
                new TieredCertificationCheckpointCurrencyLengthRule(255),
                new TieredCertificationCheckpointVersionRangeRule(86400, 86400),
            });
    }
}
