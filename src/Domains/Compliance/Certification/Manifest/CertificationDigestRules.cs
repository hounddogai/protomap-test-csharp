namespace Acme.Workforce.Compliance.Certification.Manifest;

/// <summary>A rule a certification digest must pass. Check returns its problems, or none.</summary>
public interface ICertificationDigestRule
{
    List<string> Check(CertificationDigest item);
}

public sealed record CertificationDigestStartsAtRangeRule(int Min, int Max) : ICertificationDigestRule
{
    public List<string> Check(CertificationDigest item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationDigestRequiredLabelRule(string Message) : ICertificationDigestRule
{
    public List<string> Check(CertificationDigest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationDigestRequiredPhoneRule(string Message) : ICertificationDigestRule
{
    public List<string> Check(CertificationDigest item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationDigestCeilingRangeRule(double Min, double Max) : ICertificationDigestRule
{
    public List<string> Check(CertificationDigest item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CertificationDigestLabelLengthRule(int MaxLength) : ICertificationDigestRule
{
    public List<string> Check(CertificationDigest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationDigestRequiredCodeRule(string Message) : ICertificationDigestRule
{
    public List<string> Check(CertificationDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification digest.</summary>
public sealed record CertificationDigestValidator(List<ICertificationDigestRule> Rules)
{
    public List<string> Validate(CertificationDigest item)
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

    public void RequireValid(CertificationDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationDigest> items)
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

/// <summary>Functions of certification digest rules.</summary>
public static class CertificationDigestRules
{
    public static CertificationDigestValidator DefaultCertificationDigestValidator()
    {
        return new CertificationDigestValidator(
            new List<ICertificationDigestRule>
            {
                new CertificationDigestStartsAtRangeRule(120, 1000),
                new CertificationDigestRequiredLabelRule("certification digest needs this field"),
                new CertificationDigestRequiredPhoneRule("certification digest needs a value"),
                new CertificationDigestCeilingRangeRule(7.9, 4677.7),
                new CertificationDigestLabelLengthRule(255),
                new CertificationDigestRequiredCodeRule("certification digest needs a value"),
            });
    }
}
