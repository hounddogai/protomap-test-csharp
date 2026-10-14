namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling booking review is in its life cycle.</summary>
public enum RollingBookingReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rolling booking review as the booking team keeps it.</summary>
public sealed class RollingBookingReview(string code, string name, int revision, string channel, string region, double weight, RollingBookingReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public string Channel { get; set; } = channel;
    public string Region { get; set; } = region;
    public double Weight { get; set; } = weight;
    public RollingBookingReviewStatus Status { get; set; } = status;

    /// <summary>Starts a rolling booking review in status planned.</summary>
    public static RollingBookingReview NewRollingBookingReview(string code, string name)
    {
        return new RollingBookingReview(code, name, 0, "", "", 0.0, RollingBookingReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBookingReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBookingReviewStatus next)
    {
        switch (Status)
        {
            case RollingBookingReviewStatus.Planned:
                switch (next)
                {
                    case RollingBookingReviewStatus.Scheduled:
                    case RollingBookingReviewStatus.InProgress:
                        return true;
                }
                return false;
            case RollingBookingReviewStatus.Scheduled:
                return next == RollingBookingReviewStatus.Completed;
            case RollingBookingReviewStatus.InProgress:
                return next == RollingBookingReviewStatus.Completed || next == RollingBookingReviewStatus.Cancelled;
            case RollingBookingReviewStatus.Completed:
                return next == RollingBookingReviewStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RollingBookingReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling booking review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"rolling booking review"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Weight - (double)Revision, 2);
    }

    public RollingBookingReview Copy()
    {
        return new RollingBookingReview(Code, Name, Revision, Channel, Region, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
