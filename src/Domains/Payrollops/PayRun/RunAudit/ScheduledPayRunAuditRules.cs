namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled pay run audit must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayRunAuditRule
{
    List<string> Check(ScheduledPayRunAudit item);
}

public sealed record ScheduledPayRunAuditRequiredLabelRule(string Message) : IScheduledPayRunAuditRule
{
    public List<string> Check(ScheduledPayRunAudit item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayRunAuditUnitLengthRule(int MaxLength) : IScheduledPayRunAuditRule
{
    public List<string> Check(ScheduledPayRunAudit item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayRunAuditAllowedSalaryBandRule(List<string> Allowed) : IScheduledPayRunAuditRule
{
    public List<string> Check(ScheduledPayRunAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayRunAuditTimezoneLengthRule(int MaxLength) : IScheduledPayRunAuditRule
{
    public List<string> Check(ScheduledPayRunAudit item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pay run audit.</summary>
public sealed record ScheduledPayRunAuditValidator(List<IScheduledPayRunAuditRule> Rules)
{
    public List<string> Validate(ScheduledPayRunAudit item)
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

    public void RequireValid(ScheduledPayRunAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayRunAudit> items)
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

/// <summary>Functions of scheduled pay run audit rules.</summary>
public static class ScheduledPayRunAuditRules
{
    public static ScheduledPayRunAuditValidator DefaultScheduledPayRunAuditValidator()
    {
        return new ScheduledPayRunAuditValidator(
            new List<IScheduledPayRunAuditRule>
            {
                new ScheduledPayRunAuditRequiredLabelRule("scheduled pay run audit needs this field"),
                new ScheduledPayRunAuditUnitLengthRule(80),
                new ScheduledPayRunAuditAllowedSalaryBandRule(new List<string> { "lenient" }),
                new ScheduledPayRunAuditTimezoneLengthRule(16),
            });
    }
}
