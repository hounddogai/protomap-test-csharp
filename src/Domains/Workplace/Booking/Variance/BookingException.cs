namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking exception is in its life cycle.</summary>
public enum BookingExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a booking exception and its current status.</summary>
public sealed class BookingException(string code, string label, int retries, double hours, double score, int startsAt, BookingExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public double Hours { get; set; } = hours;
    public double Score { get; set; } = score;
    public int StartsAt { get; set; } = startsAt;
    public BookingExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a booking exception in status proposed.</summary>
    public static BookingException NewBookingException(string code, string label)
    {
        return new BookingException(code, label, 0, 0.0, 0.0, 0, BookingExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingExceptionStatus next)
    {
        switch (Status)
        {
            case BookingExceptionStatus.Proposed:
                switch (next)
                {
                    case BookingExceptionStatus.Approved:
                    case BookingExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
            case BookingExceptionStatus.Approved:
                switch (next)
                {
                    case BookingExceptionStatus.Rejected:
                        return true;
                }
                return false;
            case BookingExceptionStatus.Rejected:
                switch (next)
                {
                    case BookingExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round(Hours * (double)Retries, 2);
    }

    public BookingException Copy()
    {
        return new BookingException(Code, Label, Retries, Hours, Score, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
