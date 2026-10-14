namespace Acme.Workforce.Compliance.Audit.Batch;

/// <summary>A rule a audit override must pass. Check returns its problems, or none.</summary>
public interface IAuditOverrideRule
{
    List<string> Check(AuditOverride item);
}

public sealed record AuditOverrideEmployeeIdLengthRule(int MaxLength) : IAuditOverrideRule
{
    public List<string> Check(AuditOverride item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditOverridePriorityRangeRule(int Min, int Max) : IAuditOverrideRule
{
    public List<string> Check(AuditOverride item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditOverrideRequiredEmployeeIdRule(string Message) : IAuditOverrideRule
{
    public List<string> Check(AuditOverride item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit override.</summary>
public sealed record AuditOverrideValidator(List<IAuditOverrideRule> Rules)
{
    public List<string> Validate(AuditOverride item)
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

    public void RequireValid(AuditOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditOverride> items)
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

/// <summary>Functions of audit override rules.</summary>
public static class AuditOverrideRules
{
    public static AuditOverrideValidator DefaultAuditOverrideValidator()
    {
        return new AuditOverrideValidator(
            new List<IAuditOverrideRule>
            {
                new AuditOverrideEmployeeIdLengthRule(80),
                new AuditOverridePriorityRangeRule(64, 1000),
                new AuditOverrideRequiredEmployeeIdRule("audit override needs this field"),
            });
    }
}
