namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking threshold is in its life cycle.</summary>
public enum BookingThresholdStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One booking threshold, with what the planners need to act on it.</summary>
public sealed class BookingThreshold(string code, string label, string phone, string name, double minimum, int batchSize, string externalRef, BookingThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public int BatchSize { get; set; } = batchSize;
    public string ExternalRef { get; set; } = externalRef;
    public BookingThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a booking threshold in status open.</summary>
    public static BookingThreshold NewBookingThreshold(string code, string label)
    {
        return new BookingThreshold(code, label, "", "", 0.0, 0, "", BookingThresholdStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingThresholdStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingThresholdStatus next)
    {
        switch (Status)
        {
            case BookingThresholdStatus.Open:
                return next == BookingThresholdStatus.Archived;
            case BookingThresholdStatus.InReview:
                switch (next)
                {
                    case BookingThresholdStatus.Archived:
                        return true;
                }
                return false;
            case BookingThresholdStatus.Resolved:
                switch (next)
                {
                    case BookingThresholdStatus.Archived:
                        return true;
                }
                return false;
            case BookingThresholdStatus.Reopened:
                return next == BookingThresholdStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BookingThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetLoad()
    {
        return Math.Round(Minimum * (double)BatchSize, 2);
    }

    public BookingThreshold Copy()
    {
        return new BookingThreshold(Code, Label, Phone, Name, Minimum, BatchSize, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
