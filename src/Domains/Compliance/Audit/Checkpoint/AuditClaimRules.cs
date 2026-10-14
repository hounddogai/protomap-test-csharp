namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a audit claim must pass. Check returns its problems, or none.</summary>
public interface IAuditClaimRule
{
    List<string> Check(AuditClaim item);
}

public sealed record AuditClaimCodeLengthRule(int MaxLength) : IAuditClaimRule
{
    public List<string> Check(AuditClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditClaimRequiredCodeRule(string Message) : IAuditClaimRule
{
    public List<string> Check(AuditClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditClaimAllowedNameRule(List<string> Allowed) : IAuditClaimRule
{
    public List<string> Check(AuditClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit claim.</summary>
public sealed record AuditClaimValidator(List<IAuditClaimRule> Rules)
{
    public List<string> Validate(AuditClaim item)
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

    public void RequireValid(AuditClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditClaim> items)
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

/// <summary>Functions of audit claim rules.</summary>
public static class AuditClaimRules
{
    public static AuditClaimValidator DefaultAuditClaimValidator()
    {
        return new AuditClaimValidator(
            new List<IAuditClaimRule>
            {
                new AuditClaimCodeLengthRule(80),
                new AuditClaimRequiredCodeRule("audit claim needs an entry"),
                new AuditClaimAllowedNameRule(new List<string> { "weighted", "adaptive" }),
            });
    }
}
