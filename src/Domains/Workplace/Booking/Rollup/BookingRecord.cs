namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking record is in its life cycle.</summary>
public enum BookingRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A booking record as the booking team keeps it.</summary>
public sealed class BookingRecord(string code, string label, string summary, string segment, double margin, int version, double weight, int windowDays, string salaryBand, BookingRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public string Segment { get; set; } = segment;
    public double Margin { get; set; } = margin;
    public int Version { get; set; } = version;
    public double Weight { get; set; } = weight;
    public int WindowDays { get; set; } = windowDays;
    public string SalaryBand { get; set; } = salaryBand;
    public BookingRecordStatus Status { get; set; } = status;

    /// <summary>Starts a booking record in status requested.</summary>
    public static BookingRecord NewBookingRecord(string code, string label)
    {
        return new BookingRecord(code, label, "", "", 0.0, 0, 0.0, 0, "", BookingRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingRecordStatus next)
    {
        switch (Status)
        {
            case BookingRecordStatus.Requested:
                return next == BookingRecordStatus.Granted;
            case BookingRecordStatus.Granted:
                return next == BookingRecordStatus.Revoked;
            case BookingRecordStatus.Revoked:
                return next == BookingRecordStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BookingRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"booking record"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Margin - (double)Version, 2);
    }

    public BookingRecord Copy()
    {
        return new BookingRecord(
            Code,
            Label,
            Summary,
            Segment,
            Margin,
            Version,
            Weight,
            WindowDays,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
