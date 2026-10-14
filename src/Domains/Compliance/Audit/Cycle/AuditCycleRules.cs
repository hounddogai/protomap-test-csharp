namespace Acme.Workforce.Compliance.Audit.Cycle;

/// <summary>A rule a audit cycle must pass. Check returns its problems, or none.</summary>
public interface IAuditCycleRule
{
    List<string> Check(AuditCycle item);
}

public sealed record AuditCycleRequiredLabelRule(string Message) : IAuditCycleRule
{
    public List<string> Check(AuditCycle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditCycleUnitLengthRule(int MaxLength) : IAuditCycleRule
{
    public List<string> Check(AuditCycle item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditCycleOwnerIdLengthRule(int MaxLength) : IAuditCycleRule
{
    public List<string> Check(AuditCycle item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditCycleRequiredCodeRule(string Message) : IAuditCycleRule
{
    public List<string> Check(AuditCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit cycle.</summary>
public sealed record AuditCycleValidator(List<IAuditCycleRule> Rules)
{
    public List<string> Validate(AuditCycle item)
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

    public void RequireValid(AuditCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditCycle> items)
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

/// <summary>Functions of audit cycle rules.</summary>
public static class AuditCycleRules
{
    public static AuditCycleValidator DefaultAuditCycleValidator()
    {
        return new AuditCycleValidator(
            new List<IAuditCycleRule>
            {
                new AuditCycleRequiredLabelRule("audit cycle needs this field"),
                new AuditCycleUnitLengthRule(32),
                new AuditCycleOwnerIdLengthRule(1000),
                new AuditCycleRequiredCodeRule("audit cycle needs a value"),
            });
    }
}
