namespace Acme.Workforce.Compliance.Audit.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a audit digest must pass. Check returns its problems, or none.</summary>
public interface IAuditDigestRule
{
    List<string> Check(AuditDigest item);
}

public sealed record AuditDigestRetriesRangeRule(int Min, int Max) : IAuditDigestRule
{
    public List<string> Check(AuditDigest item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AuditDigestRequiredSalaryBandRule(string Message) : IAuditDigestRule
{
    public List<string> Check(AuditDigest item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditDigestAllowedNameRule(List<string> Allowed) : IAuditDigestRule
{
    public List<string> Check(AuditDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record AuditDigestSalaryBandLengthRule(int MaxLength) : IAuditDigestRule
{
    public List<string> Check(AuditDigest item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit digest.</summary>
public sealed record AuditDigestValidator(List<IAuditDigestRule> Rules)
{
    public List<string> Validate(AuditDigest item)
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

    public void RequireValid(AuditDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditDigest> items)
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

/// <summary>Functions of audit digest rules.</summary>
public static class AuditDigestRules
{
    public static AuditDigestValidator DefaultAuditDigestValidator()
    {
        return new AuditDigestValidator(
            new List<IAuditDigestRule>
            {
                new AuditDigestRetriesRangeRule(16, 3600),
                new AuditDigestRequiredSalaryBandRule("audit digest needs this field"),
                new AuditDigestAllowedNameRule(new List<string> { "capped" }),
                new AuditDigestSalaryBandLengthRule(80),
            });
    }
}
