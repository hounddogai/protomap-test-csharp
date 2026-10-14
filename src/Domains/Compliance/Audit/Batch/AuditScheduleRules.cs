namespace Acme.Workforce.Compliance.Audit.Batch;

/// <summary>A rule a audit schedule must pass. Check returns its problems, or none.</summary>
public interface IAuditScheduleRule
{
    List<string> Check(AuditSchedule item);
}

public sealed record AuditScheduleCategoryLengthRule(int MaxLength) : IAuditScheduleRule
{
    public List<string> Check(AuditSchedule item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditScheduleRequiredCategoryRule(string Message) : IAuditScheduleRule
{
    public List<string> Check(AuditSchedule item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditScheduleRequiredLocaleRule(string Message) : IAuditScheduleRule
{
    public List<string> Check(AuditSchedule item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditScheduleLocaleLengthRule(int MaxLength) : IAuditScheduleRule
{
    public List<string> Check(AuditSchedule item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditScheduleRequiredTitleRule(string Message) : IAuditScheduleRule
{
    public List<string> Check(AuditSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit schedule.</summary>
public sealed record AuditScheduleValidator(List<IAuditScheduleRule> Rules)
{
    public List<string> Validate(AuditSchedule item)
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

    public void RequireValid(AuditSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditSchedule> items)
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

/// <summary>Functions of audit schedule rules.</summary>
public static class AuditScheduleRules
{
    public static AuditScheduleValidator DefaultAuditScheduleValidator()
    {
        return new AuditScheduleValidator(
            new List<IAuditScheduleRule>
            {
                new AuditScheduleCategoryLengthRule(64),
                new AuditScheduleRequiredCategoryRule("audit schedule needs this field"),
                new AuditScheduleRequiredLocaleRule("audit schedule needs this field"),
                new AuditScheduleLocaleLengthRule(255),
                new AuditScheduleRequiredTitleRule("audit schedule needs this field"),
            });
    }
}
