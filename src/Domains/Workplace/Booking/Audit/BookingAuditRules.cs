namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking audit must pass. Check returns its problems, or none.</summary>
public interface IBookingAuditRule
{
    List<string> Check(BookingAudit item);
}

public sealed record BookingAuditPhoneLengthRule(int MaxLength) : IBookingAuditRule
{
    public List<string> Check(BookingAudit item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingAuditRateRangeRule(double Min, double Max) : IBookingAuditRule
{
    public List<string> Check(BookingAudit item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingAuditCodeLengthRule(int MaxLength) : IBookingAuditRule
{
    public List<string> Check(BookingAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingAuditRequiredNameRule(string Message) : IBookingAuditRule
{
    public List<string> Check(BookingAudit item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingAuditAllowedNameRule(List<string> Allowed) : IBookingAuditRule
{
    public List<string> Check(BookingAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BookingAuditNameLengthRule(int MaxLength) : IBookingAuditRule
{
    public List<string> Check(BookingAudit item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking audit.</summary>
public sealed record BookingAuditValidator(List<IBookingAuditRule> Rules)
{
    public List<string> Validate(BookingAudit item)
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

    public void RequireValid(BookingAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingAudit> items)
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

/// <summary>Functions of booking audit rules.</summary>
public static class BookingAuditRules
{
    public static BookingAuditValidator DefaultBookingAuditValidator()
    {
        return new BookingAuditValidator(
            new List<IBookingAuditRule>
            {
                new BookingAuditPhoneLengthRule(16),
                new BookingAuditRateRangeRule(1.4, 4455.1),
                new BookingAuditCodeLengthRule(255),
                new BookingAuditRequiredNameRule("booking audit needs a value"),
                new BookingAuditAllowedNameRule(new List<string> { "rolling" }),
                new BookingAuditNameLengthRule(16),
            });
    }
}
