namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a certification request must pass. Check returns its problems, or none.</summary>
public interface ICertificationRequestRule
{
    List<string> Check(CertificationRequest item);
}

public sealed record CertificationRequestRequiredCodeRule(string Message) : ICertificationRequestRule
{
    public List<string> Check(CertificationRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationRequestAllowedEmployeeIdRule(List<string> Allowed) : ICertificationRequestRule
{
    public List<string> Check(CertificationRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is expired" };
        }
        return new List<string>();
    }
}

public sealed record CertificationRequestCodePrefixRule(string Prefix) : ICertificationRequestRule
{
    public List<string> Check(CertificationRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationRequestTimezoneLengthRule(int MaxLength) : ICertificationRequestRule
{
    public List<string> Check(CertificationRequest item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationRequestRequiredTimezoneRule(string Message) : ICertificationRequestRule
{
    public List<string> Check(CertificationRequest item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationRequestTitleLengthRule(int MaxLength) : ICertificationRequestRule
{
    public List<string> Check(CertificationRequest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification request.</summary>
public sealed record CertificationRequestValidator(List<ICertificationRequestRule> Rules)
{
    public List<string> Validate(CertificationRequest item)
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

    public void RequireValid(CertificationRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationRequest> items)
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

/// <summary>Functions of certification request rules.</summary>
public static class CertificationRequestRules
{
    public static CertificationRequestValidator DefaultCertificationRequestValidator()
    {
        return new CertificationRequestValidator(
            new List<ICertificationRequestRule>
            {
                new CertificationRequestRequiredCodeRule("certification request needs a value"),
                new CertificationRequestAllowedEmployeeIdRule(new List<string>()),
                new CertificationRequestCodePrefixRule("OPS"),
                new CertificationRequestTimezoneLengthRule(255),
                new CertificationRequestRequiredTimezoneRule("certification request needs an entry"),
                new CertificationRequestTitleLengthRule(1000),
            });
    }
}
