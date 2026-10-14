namespace Acme.Workforce.Compliance.Audit.Bundle;

/// <summary>A rule a audit entry must pass. Check returns its problems, or none.</summary>
public interface IAuditEntryRule
{
    List<string> Check(AuditEntry item);
}

public sealed record AuditEntryRequiredNotesRule(string Message) : IAuditEntryRule
{
    public List<string> Check(AuditEntry item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditEntryLabelLengthRule(int MaxLength) : IAuditEntryRule
{
    public List<string> Check(AuditEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditEntryCodePrefixRule(string Prefix) : IAuditEntryRule
{
    public List<string> Check(AuditEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditEntryNotesLengthRule(int MaxLength) : IAuditEntryRule
{
    public List<string> Check(AuditEntry item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditEntryRequiredBadgeRule(string Message) : IAuditEntryRule
{
    public List<string> Check(AuditEntry item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit entry.</summary>
public sealed record AuditEntryValidator(List<IAuditEntryRule> Rules)
{
    public List<string> Validate(AuditEntry item)
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

    public void RequireValid(AuditEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditEntry> items)
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

/// <summary>Functions of audit entry rules.</summary>
public static class AuditEntryRules
{
    public static AuditEntryValidator DefaultAuditEntryValidator()
    {
        return new AuditEntryValidator(
            new List<IAuditEntryRule>
            {
                new AuditEntryRequiredNotesRule("audit entry needs this field"),
                new AuditEntryLabelLengthRule(16),
                new AuditEntryCodePrefixRule("X"),
                new AuditEntryNotesLengthRule(80),
                new AuditEntryRequiredBadgeRule("audit entry needs a value"),
            });
    }
}
