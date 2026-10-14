namespace Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>A rule a regional deduction schedule must pass. Check returns its problems, or none.</summary>
public interface IRegionalDeductionScheduleRule
{
    List<string> Check(RegionalDeductionSchedule item);
}

public sealed record RegionalDeductionScheduleExternalRefLengthRule(int MaxLength) : IRegionalDeductionScheduleRule
{
    public List<string> Check(RegionalDeductionSchedule item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeductionScheduleDepartmentLengthRule(int MaxLength) : IRegionalDeductionScheduleRule
{
    public List<string> Check(RegionalDeductionSchedule item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeductionScheduleRequiredDepartmentRule(string Message) : IRegionalDeductionScheduleRule
{
    public List<string> Check(RegionalDeductionSchedule item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional deduction schedule.</summary>
public sealed record RegionalDeductionScheduleValidator(List<IRegionalDeductionScheduleRule> Rules)
{
    public List<string> Validate(RegionalDeductionSchedule item)
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

    public void RequireValid(RegionalDeductionSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalDeductionSchedule> items)
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

/// <summary>Functions of regional deduction schedule rules.</summary>
public static class RegionalDeductionScheduleRules
{
    public static RegionalDeductionScheduleValidator DefaultRegionalDeductionScheduleValidator()
    {
        return new RegionalDeductionScheduleValidator(
            new List<IRegionalDeductionScheduleRule>
            {
                new RegionalDeductionScheduleExternalRefLengthRule(16),
                new RegionalDeductionScheduleDepartmentLengthRule(80),
                new RegionalDeductionScheduleRequiredDepartmentRule("regional deduction schedule needs an entry"),
            });
    }
}
