namespace Acme.Workforce.Workplace.Booking.BookingPlan;

/// <summary>A rule a manual booking audit must pass. Check returns its problems, or none.</summary>
public interface IManualBookingAuditRule
{
    List<string> Check(ManualBookingAudit item);
}

public sealed record ManualBookingAuditRevisionRangeRule(int Min, int Max) : IManualBookingAuditRule
{
    public List<string> Check(ManualBookingAudit item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingAuditCreatedAtRangeRule(int Min, int Max) : IManualBookingAuditRule
{
    public List<string> Check(ManualBookingAudit item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingAuditPinnedGuardRule(int Limit) : IManualBookingAuditRule
{
    public List<string> Check(ManualBookingAudit item)
    {
        if (item.Pinned && item.CreatedAt > Limit)
        {
            return new List<string> { $"created at {item.CreatedAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual booking audit.</summary>
public sealed record ManualBookingAuditValidator(List<IManualBookingAuditRule> Rules)
{
    public List<string> Validate(ManualBookingAudit item)
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

    public void RequireValid(ManualBookingAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBookingAudit> items)
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

/// <summary>Functions of manual booking audit rules.</summary>
public static class ManualBookingAuditRules
{
    public static ManualBookingAuditValidator DefaultManualBookingAuditValidator()
    {
        return new ManualBookingAuditValidator(
            new List<IManualBookingAuditRule>
            {
                new ManualBookingAuditRevisionRangeRule(64, 3600),
                new ManualBookingAuditCreatedAtRangeRule(16, 120),
                new ManualBookingAuditPinnedGuardRule(64),
            });
    }
}
