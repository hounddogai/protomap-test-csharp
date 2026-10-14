namespace Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>A rule a deduction schedule must pass. Check returns its problems, or none.</summary>
public interface IDeductionScheduleRule
{
    List<string> Check(DeductionSchedule item);
}

public sealed record DeductionScheduleRequiredOwnerIdRule(string Message) : IDeductionScheduleRule
{
    public List<string> Check(DeductionSchedule item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionScheduleRequiredChecksumRule(string Message) : IDeductionScheduleRule
{
    public List<string> Check(DeductionSchedule item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionScheduleLeaveDaysRangeRule(int Min, int Max) : IDeductionScheduleRule
{
    public List<string> Check(DeductionSchedule item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeductionScheduleCodePrefixRule(string Prefix) : IDeductionScheduleRule
{
    public List<string> Check(DeductionSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction schedule.</summary>
public sealed record DeductionScheduleValidator(List<IDeductionScheduleRule> Rules)
{
    public List<string> Validate(DeductionSchedule item)
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

    public void RequireValid(DeductionSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionSchedule> items)
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

/// <summary>Functions of deduction schedule rules.</summary>
public static class DeductionScheduleRules
{
    public static DeductionScheduleValidator DefaultDeductionScheduleValidator()
    {
        return new DeductionScheduleValidator(
            new List<IDeductionScheduleRule>
            {
                new DeductionScheduleRequiredOwnerIdRule("deduction schedule needs a value"),
                new DeductionScheduleRequiredChecksumRule("deduction schedule needs an entry"),
                new DeductionScheduleLeaveDaysRangeRule(16, 3600),
                new DeductionScheduleCodePrefixRule("ACM"),
            });
    }
}
