namespace Acme.Workforce.Compliance.Audit.Adjustment;

/// <summary>A rule a primary audit schedule must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAuditScheduleRule
{
    List<string> Check(PrimaryAuditSchedule item);
}

public sealed record PrimaryAuditScheduleCodePrefixRule(string Prefix) : IPrimaryAuditScheduleRule
{
    public List<string> Check(PrimaryAuditSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAuditScheduleRequiredNameRule(string Message) : IPrimaryAuditScheduleRule
{
    public List<string> Check(PrimaryAuditSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAuditScheduleRatioRangeRule(double Min, double Max) : IPrimaryAuditScheduleRule
{
    public List<string> Check(PrimaryAuditSchedule item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryAuditScheduleDescriptionLengthRule(int MaxLength) : IPrimaryAuditScheduleRule
{
    public List<string> Check(PrimaryAuditSchedule item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAuditScheduleRequiredEmployeeIdRule(string Message) : IPrimaryAuditScheduleRule
{
    public List<string> Check(PrimaryAuditSchedule item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary audit schedule.</summary>
public sealed record PrimaryAuditScheduleValidator(List<IPrimaryAuditScheduleRule> Rules)
{
    public List<string> Validate(PrimaryAuditSchedule item)
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

    public void RequireValid(PrimaryAuditSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAuditSchedule> items)
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

/// <summary>Functions of primary audit schedule rules.</summary>
public static class PrimaryAuditScheduleRules
{
    public static PrimaryAuditScheduleValidator DefaultPrimaryAuditScheduleValidator()
    {
        return new PrimaryAuditScheduleValidator(
            new List<IPrimaryAuditScheduleRule>
            {
                new PrimaryAuditScheduleCodePrefixRule("X"),
                new PrimaryAuditScheduleRequiredNameRule("primary audit schedule needs this field"),
                new PrimaryAuditScheduleRatioRangeRule(8.5, 4691.8),
                new PrimaryAuditScheduleDescriptionLengthRule(16),
                new PrimaryAuditScheduleRequiredEmployeeIdRule("primary audit schedule needs a value"),
            });
    }
}
