namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a audit draft must pass. Check returns its problems, or none.</summary>
public interface IAuditDraftRule
{
    List<string> Check(AuditDraft item);
}

public sealed record AuditDraftRequiredTimezoneRule(string Message) : IAuditDraftRule
{
    public List<string> Check(AuditDraft item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditDraftAllowedPhoneRule(List<string> Allowed) : IAuditDraftRule
{
    public List<string> Check(AuditDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record AuditDraftCodePrefixRule(string Prefix) : IAuditDraftRule
{
    public List<string> Check(AuditDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditDraftRequiredPhoneRule(string Message) : IAuditDraftRule
{
    public List<string> Check(AuditDraft item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit draft.</summary>
public sealed record AuditDraftValidator(List<IAuditDraftRule> Rules)
{
    public List<string> Validate(AuditDraft item)
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

    public void RequireValid(AuditDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditDraft> items)
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

/// <summary>Functions of audit draft rules.</summary>
public static class AuditDraftRules
{
    public static AuditDraftValidator DefaultAuditDraftValidator()
    {
        return new AuditDraftValidator(
            new List<IAuditDraftRule>
            {
                new AuditDraftRequiredTimezoneRule("audit draft needs this field"),
                new AuditDraftAllowedPhoneRule(new List<string> { "regional", "strict" }),
                new AuditDraftCodePrefixRule("REF"),
                new AuditDraftRequiredPhoneRule("audit draft needs a value"),
            });
    }
}
