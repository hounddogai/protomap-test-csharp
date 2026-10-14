namespace Acme.Workforce.Compliance.Certification.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a certification group must pass. Check returns its problems, or none.</summary>
public interface ICertificationGroupRule
{
    List<string> Check(CertificationGroup item);
}

public sealed record CertificationGroupAllowedSummaryRule(List<string> Allowed) : ICertificationGroupRule
{
    public List<string> Check(CertificationGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is expired" };
        }
        return new List<string>();
    }
}

public sealed record CertificationGroupSummaryLengthRule(int MaxLength) : ICertificationGroupRule
{
    public List<string> Check(CertificationGroup item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationGroupCodePrefixRule(string Prefix) : ICertificationGroupRule
{
    public List<string> Check(CertificationGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationGroupEndsAtRangeRule(int Min, int Max) : ICertificationGroupRule
{
    public List<string> Check(CertificationGroup item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification group.</summary>
public sealed record CertificationGroupValidator(List<ICertificationGroupRule> Rules)
{
    public List<string> Validate(CertificationGroup item)
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

    public void RequireValid(CertificationGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationGroup> items)
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

/// <summary>Functions of certification group rules.</summary>
public static class CertificationGroupRules
{
    public static CertificationGroupValidator DefaultCertificationGroupValidator()
    {
        return new CertificationGroupValidator(
            new List<ICertificationGroupRule>
            {
                new CertificationGroupAllowedSummaryRule(new List<string> { "seasonal" }),
                new CertificationGroupSummaryLengthRule(255),
                new CertificationGroupCodePrefixRule("Q"),
                new CertificationGroupEndsAtRangeRule(16, 120),
            });
    }
}
