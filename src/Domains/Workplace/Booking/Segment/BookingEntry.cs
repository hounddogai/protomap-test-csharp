namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking entry is in its life cycle.</summary>
public enum BookingEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A booking entry as the booking team keeps it.</summary>
public sealed class BookingEntry(string code, string title, int attempts, int durationMinutes, string segment, double fte, string summary, double ceiling, Dictionary<string, string> attributes, string category, int leaveDays, BookingEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Segment { get; set; } = segment;
    public double Fte { get; set; } = fte;
    public string Summary { get; set; } = summary;
    public double Ceiling { get; set; } = ceiling;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Category { get; set; } = category;
    public int LeaveDays { get; set; } = leaveDays;
    public BookingEntryStatus Status { get; set; } = status;

    /// <summary>Starts a booking entry in status proposed.</summary>
    public static BookingEntry NewBookingEntry(string code, string title)
    {
        return new BookingEntry(
            code,
            title,
            0,
            0,
            "",
            0.0,
            "",
            0.0,
            new Dictionary<string, string>(),
            "",
            0,
            BookingEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingEntryStatus next)
    {
        switch (Status)
        {
            case BookingEntryStatus.Proposed:
                switch (next)
                {
                    case BookingEntryStatus.Approved:
                    case BookingEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case BookingEntryStatus.Approved:
                return next == BookingEntryStatus.Withdrawn;
            case BookingEntryStatus.Rejected:
                switch (next)
                {
                    case BookingEntryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"booking entry"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetVolume()
    {
        return Math.Round((double)Attempts - (double)LeaveDays, 2);
    }

    public BookingEntry Copy()
    {
        return new BookingEntry(
            Code,
            Title,
            Attempts,
            DurationMinutes,
            Segment,
            Fte,
            Summary,
            Ceiling,
            Attributes,
            Category,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
