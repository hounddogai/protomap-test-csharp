namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking tier is in its life cycle.</summary>
public enum BookingTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One booking tier, with what the planners need to act on it.</summary>
public sealed class BookingTier(string code, string name, int limit, string timezone, int tenureMonths, int version, double margin, int quantity, BookingTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public string Timezone { get; set; } = timezone;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Version { get; set; } = version;
    public double Margin { get; set; } = margin;
    public int Quantity { get; set; } = quantity;
    public BookingTierStatus Status { get; set; } = status;

    /// <summary>Starts a booking tier in status open.</summary>
    public static BookingTier NewBookingTier(string code, string name)
    {
        return new BookingTier(code, name, 0, "", 0, 0, 0.0, 0, BookingTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingTierStatus next)
    {
        switch (Status)
        {
            case BookingTierStatus.Open:
                return next == BookingTierStatus.Resolved || next == BookingTierStatus.Archived;
            case BookingTierStatus.InReview:
                switch (next)
                {
                    case BookingTierStatus.Resolved:
                        return true;
                }
                return false;
            case BookingTierStatus.Resolved:
                return next == BookingTierStatus.Reopened;
            case BookingTierStatus.Reopened:
                return next == BookingTierStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BookingTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"booking tier"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Quantity * (double)Version, 2);
    }

    public BookingTier Copy()
    {
        return new BookingTier(Code, Name, Limit, Timezone, TenureMonths, Version, Margin, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
