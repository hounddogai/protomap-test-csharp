namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking notice is in its life cycle.</summary>
public enum BookingNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A booking notice as the booking team keeps it.</summary>
public sealed class BookingNotice(string code, string title, string segment, double rating, string notes, List<string> tags, double margin, string grade, double hours, double ratio, double floorValue, BookingNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public double Rating { get; set; } = rating;
    public string Notes { get; set; } = notes;
    public List<string> Tags { get; set; } = tags;
    public double Margin { get; set; } = margin;
    public string Grade { get; set; } = grade;
    public double Hours { get; set; } = hours;
    public double Ratio { get; set; } = ratio;
    public double FloorValue { get; set; } = floorValue;
    public BookingNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a booking notice in status new.</summary>
    public static BookingNotice NewBookingNotice(string code, string title)
    {
        return new BookingNotice(
            code,
            title,
            "",
            0.0,
            "",
            new List<string>(),
            0.0,
            "",
            0.0,
            0.0,
            0.0,
            BookingNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingNoticeStatus next)
    {
        switch (Status)
        {
            case BookingNoticeStatus.New:
                switch (next)
                {
                    case BookingNoticeStatus.Running:
                    case BookingNoticeStatus.Done:
                        return true;
                }
                return false;
            case BookingNoticeStatus.Queued:
                switch (next)
                {
                    case BookingNoticeStatus.Failed:
                        return true;
                }
                return false;
            case BookingNoticeStatus.Running:
                switch (next)
                {
                    case BookingNoticeStatus.Done:
                    case BookingNoticeStatus.Failed:
                        return true;
                }
                return false;
            case BookingNoticeStatus.Done:
                return next == BookingNoticeStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BookingNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedTotal()
    {
        return Math.Round(Hours * FloorValue, 2);
    }

    public BookingNotice Copy()
    {
        return new BookingNotice(
            Code,
            Title,
            Segment,
            Rating,
            Notes,
            Tags,
            Margin,
            Grade,
            Hours,
            Ratio,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
