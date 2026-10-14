namespace Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>A rule a manual audit schedule must pass. Check returns its problems, or none.</summary>
public interface IManualAuditScheduleRule
{
    List<string> Check(ManualAuditSchedule item);
}

public sealed record ManualAuditScheduleTitleLengthRule(int MaxLength) : IManualAuditScheduleRule
{
    public List<string> Check(ManualAuditSchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAuditScheduleDiscountRangeRule(double Min, double Max) : IManualAuditScheduleRule
{
    public List<string> Check(ManualAuditSchedule item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualAuditScheduleCodeLengthRule(int MaxLength) : IManualAuditScheduleRule
{
    public List<string> Check(ManualAuditSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAuditScheduleLocaleLengthRule(int MaxLength) : IManualAuditScheduleRule
{
    public List<string> Check(ManualAuditSchedule item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual audit schedule.</summary>
public sealed record ManualAuditScheduleValidator(List<IManualAuditScheduleRule> Rules)
{
    public List<string> Validate(ManualAuditSchedule item)
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

    public void RequireValid(ManualAuditSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAuditSchedule> items)
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

/// <summary>Functions of manual audit schedule rules.</summary>
public static class ManualAuditScheduleRules
{
    public static ManualAuditScheduleValidator DefaultManualAuditScheduleValidator()
    {
        return new ManualAuditScheduleValidator(
            new List<IManualAuditScheduleRule>
            {
                new ManualAuditScheduleTitleLengthRule(64),
                new ManualAuditScheduleDiscountRangeRule(0.0, 2922.9),
                new ManualAuditScheduleCodeLengthRule(120),
                new ManualAuditScheduleLocaleLengthRule(16),
            });
    }
}
