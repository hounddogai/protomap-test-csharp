namespace Acme.Workforce.Compliance.Certification.Assignment;

/// <summary>A rule a certification checkpoint must pass. Check returns its problems, or none.</summary>
public interface ICertificationCheckpointRule
{
    List<string> Check(CertificationCheckpoint item);
}

public sealed record CertificationCheckpointLabelLengthRule(int MaxLength) : ICertificationCheckpointRule
{
    public List<string> Check(CertificationCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationCheckpointCurrencyLengthRule(int MaxLength) : ICertificationCheckpointRule
{
    public List<string> Check(CertificationCheckpoint item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationCheckpointRevisionRangeRule(int Min, int Max) : ICertificationCheckpointRule
{
    public List<string> Check(CertificationCheckpoint item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationCheckpointCodePrefixRule(string Prefix) : ICertificationCheckpointRule
{
    public List<string> Check(CertificationCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationCheckpointRequiredLabelRule(string Message) : ICertificationCheckpointRule
{
    public List<string> Check(CertificationCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification checkpoint.</summary>
public sealed record CertificationCheckpointValidator(List<ICertificationCheckpointRule> Rules)
{
    public List<string> Validate(CertificationCheckpoint item)
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

    public void RequireValid(CertificationCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationCheckpoint> items)
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

/// <summary>Functions of certification checkpoint rules.</summary>
public static class CertificationCheckpointRules
{
    public static CertificationCheckpointValidator DefaultCertificationCheckpointValidator()
    {
        return new CertificationCheckpointValidator(
            new List<ICertificationCheckpointRule>
            {
                new CertificationCheckpointLabelLengthRule(80),
                new CertificationCheckpointCurrencyLengthRule(120),
                new CertificationCheckpointRevisionRangeRule(80, 255),
                new CertificationCheckpointCodePrefixRule("ACM"),
                new CertificationCheckpointRequiredLabelRule("certification checkpoint needs a value"),
            });
    }
}
