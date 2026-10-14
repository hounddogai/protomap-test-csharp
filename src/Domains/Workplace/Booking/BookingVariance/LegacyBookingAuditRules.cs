namespace Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>A rule a legacy booking audit must pass. Check returns its problems, or none.</summary>
public interface ILegacyBookingAuditRule
{
    List<string> Check(LegacyBookingAudit item);
}

public sealed record LegacyBookingAuditQuantityRangeRule(int Min, int Max) : ILegacyBookingAuditRule
{
    public List<string> Check(LegacyBookingAudit item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyBookingAuditCreatedAtRangeRule(int Min, int Max) : ILegacyBookingAuditRule
{
    public List<string> Check(LegacyBookingAudit item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyBookingAuditRequiredOwnerIdRule(string Message) : ILegacyBookingAuditRule
{
    public List<string> Check(LegacyBookingAudit item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyBookingAuditOwnerIdLengthRule(int MaxLength) : ILegacyBookingAuditRule
{
    public List<string> Check(LegacyBookingAudit item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyBookingAuditRequiredCodeRule(string Message) : ILegacyBookingAuditRule
{
    public List<string> Check(LegacyBookingAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy booking audit.</summary>
public sealed record LegacyBookingAuditValidator(List<ILegacyBookingAuditRule> Rules)
{
    public List<string> Validate(LegacyBookingAudit item)
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

    public void RequireValid(LegacyBookingAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyBookingAudit> items)
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

/// <summary>Functions of legacy booking audit rules.</summary>
public static class LegacyBookingAuditRules
{
    public static LegacyBookingAuditValidator DefaultLegacyBookingAuditValidator()
    {
        return new LegacyBookingAuditValidator(
            new List<ILegacyBookingAuditRule>
            {
                new LegacyBookingAuditQuantityRangeRule(64, 64),
                new LegacyBookingAuditCreatedAtRangeRule(1000, 86400),
                new LegacyBookingAuditRequiredOwnerIdRule("legacy booking audit needs a value"),
                new LegacyBookingAuditOwnerIdLengthRule(16),
                new LegacyBookingAuditRequiredCodeRule("legacy booking audit needs an entry"),
            });
    }
}
