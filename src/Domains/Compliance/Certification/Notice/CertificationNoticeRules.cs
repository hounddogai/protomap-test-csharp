namespace Acme.Workforce.Compliance.Certification.Notice;

/// <summary>A rule a certification notice must pass. Check returns its problems, or none.</summary>
public interface ICertificationNoticeRule
{
    List<string> Check(CertificationNotice item);
}

public sealed record CertificationNoticeDepartmentLengthRule(int MaxLength) : ICertificationNoticeRule
{
    public List<string> Check(CertificationNotice item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationNoticeCodeLengthRule(int MaxLength) : ICertificationNoticeRule
{
    public List<string> Check(CertificationNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationNoticeCodePrefixRule(string Prefix) : ICertificationNoticeRule
{
    public List<string> Check(CertificationNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationNoticeTimezoneLengthRule(int MaxLength) : ICertificationNoticeRule
{
    public List<string> Check(CertificationNotice item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification notice.</summary>
public sealed record CertificationNoticeValidator(List<ICertificationNoticeRule> Rules)
{
    public List<string> Validate(CertificationNotice item)
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

    public void RequireValid(CertificationNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationNotice> items)
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

/// <summary>Functions of certification notice rules.</summary>
public static class CertificationNoticeRules
{
    public static CertificationNoticeValidator DefaultCertificationNoticeValidator()
    {
        return new CertificationNoticeValidator(
            new List<ICertificationNoticeRule>
            {
                new CertificationNoticeDepartmentLengthRule(3600),
                new CertificationNoticeCodeLengthRule(3600),
                new CertificationNoticeCodePrefixRule("OPS"),
                new CertificationNoticeTimezoneLengthRule(16),
            });
    }
}
