namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking rule is in its life cycle.</summary>
public enum BookingRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One booking rule, with what the planners need to act on it.</summary>
public sealed class BookingRule(string code, string name, string region, double rate, string locale, BookingRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public double Rate { get; set; } = rate;
    public string Locale { get; set; } = locale;
    public BookingRuleStatus Status { get; set; } = status;

    /// <summary>Starts a booking rule in status planned.</summary>
    public static BookingRule NewBookingRule(string code, string name)
    {
        return new BookingRule(code, name, "", 0.0, "", BookingRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingRuleStatus next)
    {
        switch (Status)
        {
            case BookingRuleStatus.Planned:
                switch (next)
                {
                    case BookingRuleStatus.InProgress:
                        return true;
                }
                return false;
            case BookingRuleStatus.Scheduled:
                switch (next)
                {
                    case BookingRuleStatus.Completed:
                    case BookingRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case BookingRuleStatus.InProgress:
                return next == BookingRuleStatus.Completed || next == BookingRuleStatus.Cancelled;
            case BookingRuleStatus.Completed:
                return next == BookingRuleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BookingRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public BookingRule Copy()
    {
        return new BookingRule(Code, Name, Region, Rate, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
