namespace Acme.Workforce.Compliance.Audit.AuditMapping;

/// <summary>A rule a audit ledger must pass. Check returns its problems, or none.</summary>
public interface IAuditLedgerRule
{
    List<string> Check(AuditLedger item);
}

public sealed record AuditLedgerHoursRangeRule(double Min, double Max) : IAuditLedgerRule
{
    public List<string> Check(AuditLedger item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditLedgerRequiredDescriptionRule(string Message) : IAuditLedgerRule
{
    public List<string> Check(AuditLedger item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditLedgerRequiredCodeRule(string Message) : IAuditLedgerRule
{
    public List<string> Check(AuditLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditLedgerRequiredLocaleRule(string Message) : IAuditLedgerRule
{
    public List<string> Check(AuditLedger item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditLedgerEndsAtRangeRule(int Min, int Max) : IAuditLedgerRule
{
    public List<string> Check(AuditLedger item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditLedgerLocaleLengthRule(int MaxLength) : IAuditLedgerRule
{
    public List<string> Check(AuditLedger item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit ledger.</summary>
public sealed record AuditLedgerValidator(List<IAuditLedgerRule> Rules)
{
    public List<string> Validate(AuditLedger item)
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

    public void RequireValid(AuditLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditLedger> items)
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

/// <summary>Functions of audit ledger rules.</summary>
public static class AuditLedgerRules
{
    public static AuditLedgerValidator DefaultAuditLedgerValidator()
    {
        return new AuditLedgerValidator(
            new List<IAuditLedgerRule>
            {
                new AuditLedgerHoursRangeRule(7.5, 297.4),
                new AuditLedgerRequiredDescriptionRule("audit ledger needs this field"),
                new AuditLedgerRequiredCodeRule("audit ledger needs a value"),
                new AuditLedgerRequiredLocaleRule("audit ledger needs this field"),
                new AuditLedgerEndsAtRangeRule(32, 80),
                new AuditLedgerLocaleLengthRule(120),
            });
    }
}
