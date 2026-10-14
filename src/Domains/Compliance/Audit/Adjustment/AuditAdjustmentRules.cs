namespace Acme.Workforce.Compliance.Audit.Adjustment;

/// <summary>A rule a audit adjustment must pass. Check returns its problems, or none.</summary>
public interface IAuditAdjustmentRule
{
    List<string> Check(AuditAdjustment item);
}

public sealed record AuditAdjustmentLabelLengthRule(int MaxLength) : IAuditAdjustmentRule
{
    public List<string> Check(AuditAdjustment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditAdjustmentRequiredCodeRule(string Message) : IAuditAdjustmentRule
{
    public List<string> Check(AuditAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditAdjustmentEndsAtRangeRule(int Min, int Max) : IAuditAdjustmentRule
{
    public List<string> Check(AuditAdjustment item)
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

/// <summary>Runs every rule of a audit adjustment.</summary>
public sealed record AuditAdjustmentValidator(List<IAuditAdjustmentRule> Rules)
{
    public List<string> Validate(AuditAdjustment item)
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

    public void RequireValid(AuditAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditAdjustment> items)
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

/// <summary>Functions of audit adjustment rules.</summary>
public static class AuditAdjustmentRules
{
    public static AuditAdjustmentValidator DefaultAuditAdjustmentValidator()
    {
        return new AuditAdjustmentValidator(
            new List<IAuditAdjustmentRule>
            {
                new AuditAdjustmentLabelLengthRule(1000),
                new AuditAdjustmentRequiredCodeRule("audit adjustment needs a value"),
                new AuditAdjustmentEndsAtRangeRule(1000, 86400),
            });
    }
}
