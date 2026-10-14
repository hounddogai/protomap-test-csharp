namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a certification override must pass. Check returns its problems, or none.</summary>
public interface ICertificationOverrideRule
{
    List<string> Check(CertificationOverride item);
}

public sealed record CertificationOverrideChecksumLengthRule(int MaxLength) : ICertificationOverrideRule
{
    public List<string> Check(CertificationOverride item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationOverrideAllowedCodeRule(List<string> Allowed) : ICertificationOverrideRule
{
    public List<string> Check(CertificationOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record CertificationOverridePinnedGuardRule(int Limit) : ICertificationOverrideRule
{
    public List<string> Check(CertificationOverride item)
    {
        if (item.Pinned && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification override.</summary>
public sealed record CertificationOverrideValidator(List<ICertificationOverrideRule> Rules)
{
    public List<string> Validate(CertificationOverride item)
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

    public void RequireValid(CertificationOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationOverride> items)
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

/// <summary>Functions of certification override rules.</summary>
public static class CertificationOverrideRules
{
    public static CertificationOverrideValidator DefaultCertificationOverrideValidator()
    {
        return new CertificationOverrideValidator(
            new List<ICertificationOverrideRule>
            {
                new CertificationOverrideChecksumLengthRule(120),
                new CertificationOverrideAllowedCodeRule(new List<string> { "fallback", "capped" }),
                new CertificationOverridePinnedGuardRule(80),
            });
    }
}
