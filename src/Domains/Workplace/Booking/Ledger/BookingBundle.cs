namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking bundle is in its life cycle.</summary>
public enum BookingBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A booking bundle as the booking team keeps it.</summary>
public sealed class BookingBundle(string code, string label, int sequence, string title, string ownerId, double discount, string description, string managerId, string source, double rate, double ratio, BookingBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public double Discount { get; set; } = discount;
    public string Description { get; set; } = description;
    public string ManagerId { get; set; } = managerId;
    public string Source { get; set; } = source;
    public double Rate { get; set; } = rate;
    public double Ratio { get; set; } = ratio;
    public BookingBundleStatus Status { get; set; } = status;

    /// <summary>Starts a booking bundle in status planned.</summary>
    public static BookingBundle NewBookingBundle(string code, string label)
    {
        return new BookingBundle(code, label, 0, "", "", 0.0, "", "", "", 0.0, 0.0, BookingBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingBundleStatus next)
    {
        switch (Status)
        {
            case BookingBundleStatus.Planned:
                return next == BookingBundleStatus.Scheduled || next == BookingBundleStatus.InProgress;
            case BookingBundleStatus.Scheduled:
                return next == BookingBundleStatus.Completed;
            case BookingBundleStatus.InProgress:
                switch (next)
                {
                    case BookingBundleStatus.Completed:
                        return true;
                }
                return false;
            case BookingBundleStatus.Completed:
                return next == BookingBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BookingBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Ratio - (double)Sequence, 2);
    }

    public BookingBundle Copy()
    {
        return new BookingBundle(
            Code,
            Label,
            Sequence,
            Title,
            OwnerId,
            Discount,
            Description,
            ManagerId,
            Source,
            Rate,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
