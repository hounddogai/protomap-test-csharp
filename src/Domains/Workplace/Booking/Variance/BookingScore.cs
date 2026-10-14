namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking score is in its life cycle.</summary>
public enum BookingScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A booking score. Its code is unique within a tenant.</summary>
public sealed class BookingScore(string code, string label, double fte, bool visible, int leaveDays, BookingScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public bool Visible { get; set; } = visible;
    public int LeaveDays { get; set; } = leaveDays;
    public BookingScoreStatus Status { get; set; } = status;

    /// <summary>Starts a booking score in status requested.</summary>
    public static BookingScore NewBookingScore(string code, string label)
    {
        return new BookingScore(code, label, 0.0, false, 0, BookingScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingScoreStatus next)
    {
        switch (Status)
        {
            case BookingScoreStatus.Requested:
                switch (next)
                {
                    case BookingScoreStatus.Revoked:
                    case BookingScoreStatus.Expired:
                        return true;
                }
                return false;
            case BookingScoreStatus.Granted:
                return next == BookingScoreStatus.Revoked || next == BookingScoreStatus.Expired;
            case BookingScoreStatus.Revoked:
                switch (next)
                {
                    case BookingScoreStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)LeaveDays + Fte, 2);
    }

    public BookingScore Copy()
    {
        return new BookingScore(Code, Label, Fte, Visible, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
