namespace Acme.Workforce.Compliance.Audit.Preference;

/// <summary>A rule a scheduled audit plan must pass. Check returns its problems, or none.</summary>
public interface IScheduledAuditPlanRule
{
    List<string> Check(ScheduledAuditPlan item);
}

public sealed record ScheduledAuditPlanDurationMinutesRangeRule(int Min, int Max) : IScheduledAuditPlanRule
{
    public List<string> Check(ScheduledAuditPlan item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAuditPlanApprovedGuardRule(int Limit) : IScheduledAuditPlanRule
{
    public List<string> Check(ScheduledAuditPlan item)
    {
        if (item.Approved && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAuditPlanRevisionRangeRule(int Min, int Max) : IScheduledAuditPlanRule
{
    public List<string> Check(ScheduledAuditPlan item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledAuditPlanRequiredNameRule(string Message) : IScheduledAuditPlanRule
{
    public List<string> Check(ScheduledAuditPlan item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAuditPlanCodeLengthRule(int MaxLength) : IScheduledAuditPlanRule
{
    public List<string> Check(ScheduledAuditPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled audit plan.</summary>
public sealed record ScheduledAuditPlanValidator(List<IScheduledAuditPlanRule> Rules)
{
    public List<string> Validate(ScheduledAuditPlan item)
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

    public void RequireValid(ScheduledAuditPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAuditPlan> items)
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

/// <summary>Functions of scheduled audit plan rules.</summary>
public static class ScheduledAuditPlanRules
{
    public static ScheduledAuditPlanValidator DefaultScheduledAuditPlanValidator()
    {
        return new ScheduledAuditPlanValidator(
            new List<IScheduledAuditPlanRule>
            {
                new ScheduledAuditPlanDurationMinutesRangeRule(64, 120),
                new ScheduledAuditPlanApprovedGuardRule(1000),
                new ScheduledAuditPlanRevisionRangeRule(16, 32),
                new ScheduledAuditPlanRequiredNameRule("scheduled audit plan needs a value"),
                new ScheduledAuditPlanCodeLengthRule(1000),
            });
    }
}
