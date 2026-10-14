namespace Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>A rule a pending booking audit must pass. Check returns its problems, or none.</summary>
public interface IPendingBookingAuditRule
{
    List<string> Check(PendingBookingAudit item);
}

public sealed record PendingBookingAuditRequiredNameRule(string Message) : IPendingBookingAuditRule
{
    public List<string> Check(PendingBookingAudit item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingBookingAuditRequiredCodeRule(string Message) : IPendingBookingAuditRule
{
    public List<string> Check(PendingBookingAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingBookingAuditMaximumRangeRule(double Min, double Max) : IPendingBookingAuditRule
{
    public List<string> Check(PendingBookingAudit item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBookingAuditCodePrefixRule(string Prefix) : IPendingBookingAuditRule
{
    public List<string> Check(PendingBookingAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBookingAuditGraceDaysRangeRule(int Min, int Max) : IPendingBookingAuditRule
{
    public List<string> Check(PendingBookingAudit item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending booking audit.</summary>
public sealed record PendingBookingAuditValidator(List<IPendingBookingAuditRule> Rules)
{
    public List<string> Validate(PendingBookingAudit item)
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

    public void RequireValid(PendingBookingAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingBookingAudit> items)
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

/// <summary>Functions of pending booking audit rules.</summary>
public static class PendingBookingAuditRules
{
    public static PendingBookingAuditValidator DefaultPendingBookingAuditValidator()
    {
        return new PendingBookingAuditValidator(
            new List<IPendingBookingAuditRule>
            {
                new PendingBookingAuditRequiredNameRule("pending booking audit needs an entry"),
                new PendingBookingAuditRequiredCodeRule("pending booking audit needs an entry"),
                new PendingBookingAuditMaximumRangeRule(5.6, 3448.6),
                new PendingBookingAuditCodePrefixRule("OPS"),
                new PendingBookingAuditGraceDaysRangeRule(80, 1000),
            });
    }
}
