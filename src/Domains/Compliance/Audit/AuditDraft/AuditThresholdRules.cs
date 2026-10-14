namespace Acme.Workforce.Compliance.Audit.AuditDraft;

/// <summary>A rule a audit threshold must pass. Check returns its problems, or none.</summary>
public interface IAuditThresholdRule
{
    List<string> Check(AuditThreshold item);
}

public sealed record AuditThresholdCodePrefixRule(string Prefix) : IAuditThresholdRule
{
    public List<string> Check(AuditThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditThresholdQuantityRangeRule(int Min, int Max) : IAuditThresholdRule
{
    public List<string> Check(AuditThreshold item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AuditThresholdNameLengthRule(int MaxLength) : IAuditThresholdRule
{
    public List<string> Check(AuditThreshold item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditThresholdRequiredDepartmentRule(string Message) : IAuditThresholdRule
{
    public List<string> Check(AuditThreshold item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditThresholdCodeLengthRule(int MaxLength) : IAuditThresholdRule
{
    public List<string> Check(AuditThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditThresholdDepartmentLengthRule(int MaxLength) : IAuditThresholdRule
{
    public List<string> Check(AuditThreshold item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit threshold.</summary>
public sealed record AuditThresholdValidator(List<IAuditThresholdRule> Rules)
{
    public List<string> Validate(AuditThreshold item)
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

    public void RequireValid(AuditThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditThreshold> items)
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

/// <summary>Functions of audit threshold rules.</summary>
public static class AuditThresholdRules
{
    public static AuditThresholdValidator DefaultAuditThresholdValidator()
    {
        return new AuditThresholdValidator(
            new List<IAuditThresholdRule>
            {
                new AuditThresholdCodePrefixRule("X"),
                new AuditThresholdQuantityRangeRule(80, 86400),
                new AuditThresholdNameLengthRule(64),
                new AuditThresholdRequiredDepartmentRule("audit threshold needs an entry"),
                new AuditThresholdCodeLengthRule(64),
                new AuditThresholdDepartmentLengthRule(3600),
            });
    }
}
