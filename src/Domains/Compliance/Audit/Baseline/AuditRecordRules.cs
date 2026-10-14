namespace Acme.Workforce.Compliance.Audit.Baseline;

/// <summary>A rule a audit record must pass. Check returns its problems, or none.</summary>
public interface IAuditRecordRule
{
    List<string> Check(AuditRecord item);
}

public sealed record AuditRecordCodeLengthRule(int MaxLength) : IAuditRecordRule
{
    public List<string> Check(AuditRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditRecordReviewerLengthRule(int MaxLength) : IAuditRecordRule
{
    public List<string> Check(AuditRecord item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditRecordRequiredCodeRule(string Message) : IAuditRecordRule
{
    public List<string> Check(AuditRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditRecordLimitRangeRule(int Min, int Max) : IAuditRecordRule
{
    public List<string> Check(AuditRecord item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AuditRecordRateRangeRule(double Min, double Max) : IAuditRecordRule
{
    public List<string> Check(AuditRecord item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit record.</summary>
public sealed record AuditRecordValidator(List<IAuditRecordRule> Rules)
{
    public List<string> Validate(AuditRecord item)
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

    public void RequireValid(AuditRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditRecord> items)
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

/// <summary>Functions of audit record rules.</summary>
public static class AuditRecordRules
{
    public static AuditRecordValidator DefaultAuditRecordValidator()
    {
        return new AuditRecordValidator(
            new List<IAuditRecordRule>
            {
                new AuditRecordCodeLengthRule(1000),
                new AuditRecordReviewerLengthRule(80),
                new AuditRecordRequiredCodeRule("audit record needs this field"),
                new AuditRecordLimitRangeRule(80, 3600),
                new AuditRecordRateRangeRule(3.5, 3355.7),
            });
    }
}
