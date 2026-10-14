namespace Acme.Workforce.Compliance.Audit.AuditRecord;

/// <summary>A rule a strict audit record must pass. Check returns its problems, or none.</summary>
public interface IStrictAuditRecordRule
{
    List<string> Check(StrictAuditRecord item);
}

public sealed record StrictAuditRecordSalaryBandLengthRule(int MaxLength) : IStrictAuditRecordRule
{
    public List<string> Check(StrictAuditRecord item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAuditRecordTitleLengthRule(int MaxLength) : IStrictAuditRecordRule
{
    public List<string> Check(StrictAuditRecord item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAuditRecordRequiredTitleRule(string Message) : IStrictAuditRecordRule
{
    public List<string> Check(StrictAuditRecord item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict audit record.</summary>
public sealed record StrictAuditRecordValidator(List<IStrictAuditRecordRule> Rules)
{
    public List<string> Validate(StrictAuditRecord item)
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

    public void RequireValid(StrictAuditRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAuditRecord> items)
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

/// <summary>Functions of strict audit record rules.</summary>
public static class StrictAuditRecordRules
{
    public static StrictAuditRecordValidator DefaultStrictAuditRecordValidator()
    {
        return new StrictAuditRecordValidator(
            new List<IStrictAuditRecordRule>
            {
                new StrictAuditRecordSalaryBandLengthRule(64),
                new StrictAuditRecordTitleLengthRule(32),
                new StrictAuditRecordRequiredTitleRule("strict audit record needs a value"),
            });
    }
}
