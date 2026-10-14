namespace Acme.Workforce.Compliance.Certification.Schedule;

/// <summary>A rule a certification policy must pass. Check returns its problems, or none.</summary>
public interface ICertificationPolicyRule
{
    List<string> Check(CertificationPolicy item);
}

public sealed record CertificationPolicyVersionRangeRule(int Min, int Max) : ICertificationPolicyRule
{
    public List<string> Check(CertificationPolicy item)
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

public sealed record CertificationPolicyCodePrefixRule(string Prefix) : ICertificationPolicyRule
{
    public List<string> Check(CertificationPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationPolicyRequiredNameRule(string Message) : ICertificationPolicyRule
{
    public List<string> Check(CertificationPolicy item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationPolicyPriorityRangeRule(int Min, int Max) : ICertificationPolicyRule
{
    public List<string> Check(CertificationPolicy item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationPolicyNameLengthRule(int MaxLength) : ICertificationPolicyRule
{
    public List<string> Check(CertificationPolicy item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification policy.</summary>
public sealed record CertificationPolicyValidator(List<ICertificationPolicyRule> Rules)
{
    public List<string> Validate(CertificationPolicy item)
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

    public void RequireValid(CertificationPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationPolicy> items)
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

/// <summary>Functions of certification policy rules.</summary>
public static class CertificationPolicyRules
{
    public static CertificationPolicyValidator DefaultCertificationPolicyValidator()
    {
        return new CertificationPolicyValidator(
            new List<ICertificationPolicyRule>
            {
                new CertificationPolicyVersionRangeRule(64, 1000),
                new CertificationPolicyCodePrefixRule("REF"),
                new CertificationPolicyRequiredNameRule("certification policy needs this field"),
                new CertificationPolicyPriorityRangeRule(32, 3600),
                new CertificationPolicyNameLengthRule(16),
            });
    }
}
