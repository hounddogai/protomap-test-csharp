namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking adjustment must pass. Check returns its problems, or none.</summary>
public interface IBookingAdjustmentRule
{
    List<string> Check(BookingAdjustment item);
}

public sealed record BookingAdjustmentAllowedCodeRule(List<string> Allowed) : IBookingAdjustmentRule
{
    public List<string> Check(BookingAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record BookingAdjustmentLabelLengthRule(int MaxLength) : IBookingAdjustmentRule
{
    public List<string> Check(BookingAdjustment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingAdjustmentCreatedAtRangeRule(int Min, int Max) : IBookingAdjustmentRule
{
    public List<string> Check(BookingAdjustment item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingAdjustmentSegmentLengthRule(int MaxLength) : IBookingAdjustmentRule
{
    public List<string> Check(BookingAdjustment item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingAdjustmentActiveGuardRule(double Limit) : IBookingAdjustmentRule
{
    public List<string> Check(BookingAdjustment item)
    {
        if (item.Active && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record BookingAdjustmentRequiredSegmentRule(string Message) : IBookingAdjustmentRule
{
    public List<string> Check(BookingAdjustment item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking adjustment.</summary>
public sealed record BookingAdjustmentValidator(List<IBookingAdjustmentRule> Rules)
{
    public List<string> Validate(BookingAdjustment item)
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

    public void RequireValid(BookingAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingAdjustment> items)
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

/// <summary>Functions of booking adjustment rules.</summary>
public static class BookingAdjustmentRules
{
    public static BookingAdjustmentValidator DefaultBookingAdjustmentValidator()
    {
        return new BookingAdjustmentValidator(
            new List<IBookingAdjustmentRule>
            {
                new BookingAdjustmentAllowedCodeRule(new List<string> { "fallback", "tiered" }),
                new BookingAdjustmentLabelLengthRule(86400),
                new BookingAdjustmentCreatedAtRangeRule(16, 80),
                new BookingAdjustmentSegmentLengthRule(64),
                new BookingAdjustmentActiveGuardRule(3.8),
                new BookingAdjustmentRequiredSegmentRule("booking adjustment needs a value"),
            });
    }
}
