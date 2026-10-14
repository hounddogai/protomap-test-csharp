namespace Acme.Workforce.Compliance.Attestation.Audit;

/// <summary>A rule a attestation audit must pass. Check returns its problems, or none.</summary>
public interface IAttestationAuditRule
{
    List<string> Check(AttestationAudit item);
}

public sealed record AttestationAuditCodePrefixRule(string Prefix) : IAttestationAuditRule
{
    public List<string> Check(AttestationAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationAuditRequiredTitleRule(string Message) : IAttestationAuditRule
{
    public List<string> Check(AttestationAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationAuditRequiredTimezoneRule(string Message) : IAttestationAuditRule
{
    public List<string> Check(AttestationAudit item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation audit.</summary>
public sealed record AttestationAuditValidator(List<IAttestationAuditRule> Rules)
{
    public List<string> Validate(AttestationAudit item)
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

    public void RequireValid(AttestationAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationAudit> items)
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

/// <summary>Functions of attestation audit rules.</summary>
public static class AttestationAuditRules
{
    public static AttestationAuditValidator DefaultAttestationAuditValidator()
    {
        return new AttestationAuditValidator(
            new List<IAttestationAuditRule>
            {
                new AttestationAuditCodePrefixRule("X"),
                new AttestationAuditRequiredTitleRule("attestation audit needs this field"),
                new AttestationAuditRequiredTimezoneRule("attestation audit needs a value"),
            });
    }
}
