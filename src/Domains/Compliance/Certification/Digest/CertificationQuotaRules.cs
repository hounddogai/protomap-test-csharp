namespace Acme.Workforce.Compliance.Certification.Digest;

/// <summary>A rule a certification quota must pass. Check returns its problems, or none.</summary>
public interface ICertificationQuotaRule
{
    List<string> Check(CertificationQuota item);
}

public sealed record CertificationQuotaTitleLengthRule(int MaxLength) : ICertificationQuotaRule
{
    public List<string> Check(CertificationQuota item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationQuotaUpdatedAtRangeRule(int Min, int Max) : ICertificationQuotaRule
{
    public List<string> Check(CertificationQuota item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationQuotaExternalRefLengthRule(int MaxLength) : ICertificationQuotaRule
{
    public List<string> Check(CertificationQuota item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification quota.</summary>
public sealed record CertificationQuotaValidator(List<ICertificationQuotaRule> Rules)
{
    public List<string> Validate(CertificationQuota item)
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

    public void RequireValid(CertificationQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationQuota> items)
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

/// <summary>Functions of certification quota rules.</summary>
public static class CertificationQuotaRules
{
    public static CertificationQuotaValidator DefaultCertificationQuotaValidator()
    {
        return new CertificationQuotaValidator(
            new List<ICertificationQuotaRule>
            {
                new CertificationQuotaTitleLengthRule(16),
                new CertificationQuotaUpdatedAtRangeRule(32, 255),
                new CertificationQuotaExternalRefLengthRule(1000),
            });
    }
}
