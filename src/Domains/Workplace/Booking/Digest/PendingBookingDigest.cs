namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending booking digest is in its life cycle.</summary>
public enum PendingBookingDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One pending booking digest, with what the planners need to act on it.</summary>
public sealed class PendingBookingDigest(string code, string label, double threshold, string name, int version, string summary, int capacity, string category, int graceDays, PendingBookingDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string Summary { get; set; } = summary;
    public int Capacity { get; set; } = capacity;
    public string Category { get; set; } = category;
    public int GraceDays { get; set; } = graceDays;
    public PendingBookingDigestStatus Status { get; set; } = status;

    /// <summary>Starts a pending booking digest in status planned.</summary>
    public static PendingBookingDigest NewPendingBookingDigest(string code, string label)
    {
        return new PendingBookingDigest(code, label, 0.0, "", 0, "", 0, "", 0, PendingBookingDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingBookingDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingBookingDigestStatus next)
    {
        switch (Status)
        {
            case PendingBookingDigestStatus.Planned:
                switch (next)
                {
                    case PendingBookingDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingBookingDigestStatus.Scheduled:
                return next == PendingBookingDigestStatus.Completed || next == PendingBookingDigestStatus.Cancelled;
            case PendingBookingDigestStatus.InProgress:
                switch (next)
                {
                    case PendingBookingDigestStatus.Completed:
                    case PendingBookingDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingBookingDigestStatus.Completed:
                return next == PendingBookingDigestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PendingBookingDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending booking digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"pending booking digest"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)GraceDays * (double)Capacity, 2);
    }

    public PendingBookingDigest Copy()
    {
        return new PendingBookingDigest(
            Code,
            Label,
            Threshold,
            Name,
            Version,
            Summary,
            Capacity,
            Category,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
