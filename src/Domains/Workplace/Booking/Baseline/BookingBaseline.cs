namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking baseline is in its life cycle.</summary>
public enum BookingBaselineStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A booking baseline as the booking team keeps it.</summary>
public sealed class BookingBaseline(string code, string label, double ratio, int attempts, double fte, string segment, BookingBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public int Attempts { get; set; } = attempts;
    public double Fte { get; set; } = fte;
    public string Segment { get; set; } = segment;
    public BookingBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a booking baseline in status draft.</summary>
    public static BookingBaseline NewBookingBaseline(string code, string label)
    {
        return new BookingBaseline(code, label, 0.0, 0, 0.0, "", BookingBaselineStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingBaselineStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingBaselineStatus next)
    {
        switch (Status)
        {
            case BookingBaselineStatus.Draft:
                switch (next)
                {
                    case BookingBaselineStatus.Pending:
                    case BookingBaselineStatus.Closed:
                        return true;
                }
                return false;
            case BookingBaselineStatus.Pending:
                return next == BookingBaselineStatus.Active || next == BookingBaselineStatus.Suspended;
            case BookingBaselineStatus.Active:
                switch (next)
                {
                    case BookingBaselineStatus.Suspended:
                    case BookingBaselineStatus.Closed:
                        return true;
                }
                return false;
            case BookingBaselineStatus.Suspended:
                switch (next)
                {
                    case BookingBaselineStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Attempts - Ratio, 2);
    }

    public BookingBaseline Copy()
    {
        return new BookingBaseline(Code, Label, Ratio, Attempts, Fte, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
