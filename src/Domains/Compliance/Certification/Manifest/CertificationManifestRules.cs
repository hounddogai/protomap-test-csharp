namespace Acme.Workforce.Compliance.Certification.Manifest;

/// <summary>A rule a certification manifest must pass. Check returns its problems, or none.</summary>
public interface ICertificationManifestRule
{
    List<string> Check(CertificationManifest item);
}

public sealed record CertificationManifestCodeLengthRule(int MaxLength) : ICertificationManifestRule
{
    public List<string> Check(CertificationManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationManifestCreatedAtRangeRule(int Min, int Max) : ICertificationManifestRule
{
    public List<string> Check(CertificationManifest item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationManifestRequiredExternalRefRule(string Message) : ICertificationManifestRule
{
    public List<string> Check(CertificationManifest item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationManifestRequiredCodeRule(string Message) : ICertificationManifestRule
{
    public List<string> Check(CertificationManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification manifest.</summary>
public sealed record CertificationManifestValidator(List<ICertificationManifestRule> Rules)
{
    public List<string> Validate(CertificationManifest item)
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

    public void RequireValid(CertificationManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationManifest> items)
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

/// <summary>Functions of certification manifest rules.</summary>
public static class CertificationManifestRules
{
    public static CertificationManifestValidator DefaultCertificationManifestValidator()
    {
        return new CertificationManifestValidator(
            new List<ICertificationManifestRule>
            {
                new CertificationManifestCodeLengthRule(86400),
                new CertificationManifestCreatedAtRangeRule(64, 86400),
                new CertificationManifestRequiredExternalRefRule("certification manifest needs this field"),
                new CertificationManifestRequiredCodeRule("certification manifest needs a value"),
            });
    }
}
