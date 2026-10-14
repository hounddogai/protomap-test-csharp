namespace Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>A rule a booking reservation must pass. Check returns its problems, or none.</summary>
public interface IBookingReservationRule
{
    List<string> Check(BookingReservation item);
}

public sealed record BookingReservationCodeLengthRule(int MaxLength) : IBookingReservationRule
{
    public List<string> Check(BookingReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingReservationRequiredBadgeRule(string Message) : IBookingReservationRule
{
    public List<string> Check(BookingReservation item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingReservationMarginRangeRule(double Min, double Max) : IBookingReservationRule
{
    public List<string> Check(BookingReservation item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BookingReservationRequiredCodeRule(string Message) : IBookingReservationRule
{
    public List<string> Check(BookingReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking reservation.</summary>
public sealed record BookingReservationValidator(List<IBookingReservationRule> Rules)
{
    public List<string> Validate(BookingReservation item)
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

    public void RequireValid(BookingReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingReservation> items)
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

/// <summary>Functions of booking reservation rules.</summary>
public static class BookingReservationRules
{
    public static BookingReservationValidator DefaultBookingReservationValidator()
    {
        return new BookingReservationValidator(
            new List<IBookingReservationRule>
            {
                new BookingReservationCodeLengthRule(120),
                new BookingReservationRequiredBadgeRule("booking reservation needs an entry"),
                new BookingReservationMarginRangeRule(1.4, 276.5),
                new BookingReservationRequiredCodeRule("booking reservation needs an entry"),
            });
    }
}
