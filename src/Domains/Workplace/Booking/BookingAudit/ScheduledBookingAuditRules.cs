namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled booking audit must pass. Check returns its problems, or none.</summary>
public interface IScheduledBookingAuditRule
{
    List<string> Check(ScheduledBookingAudit item);
}

public sealed record ScheduledBookingAuditAllowedCodeRule(List<string> Allowed) : IScheduledBookingAuditRule
{
    public List<string> Check(ScheduledBookingAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingAuditCostRangeRule(double Min, double Max) : IScheduledBookingAuditRule
{
    public List<string> Check(ScheduledBookingAudit item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingAuditTagsLimitRule(int MaxCount, List<string> Banned) : IScheduledBookingAuditRule
{
    public List<string> Check(ScheduledBookingAudit item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record ScheduledBookingAuditCodePrefixRule(string Prefix) : IScheduledBookingAuditRule
{
    public List<string> Check(ScheduledBookingAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled booking audit.</summary>
public sealed record ScheduledBookingAuditValidator(List<IScheduledBookingAuditRule> Rules)
{
    public List<string> Validate(ScheduledBookingAudit item)
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

    public void RequireValid(ScheduledBookingAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledBookingAudit> items)
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

/// <summary>Functions of scheduled booking audit rules.</summary>
public static class ScheduledBookingAuditRules
{
    public static ScheduledBookingAuditValidator DefaultScheduledBookingAuditValidator()
    {
        return new ScheduledBookingAuditValidator(
            new List<IScheduledBookingAuditRule>
            {
                new ScheduledBookingAuditAllowedCodeRule(new List<string> { "weighted", "capped" }),
                new ScheduledBookingAuditCostRangeRule(5.8, 1801.4),
                new ScheduledBookingAuditTagsLimitRule(86400, new List<string>()),
                new ScheduledBookingAuditCodePrefixRule("OPS"),
            });
    }
}
