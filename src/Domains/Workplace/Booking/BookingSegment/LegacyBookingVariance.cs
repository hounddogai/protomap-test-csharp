namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy booking variance is in its life cycle.</summary>
public enum LegacyBookingVarianceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A legacy booking variance as the booking team keeps it.</summary>
public sealed class LegacyBookingVariance(string code, string name, int batchSize, int startsAt, string label, bool visible, string grade, string managerId, LegacyBookingVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public int StartsAt { get; set; } = startsAt;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public LegacyBookingVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a legacy booking variance in status open.</summary>
    public static LegacyBookingVariance NewLegacyBookingVariance(string code, string name)
    {
        return new LegacyBookingVariance(code, name, 0, 0, "", false, "", "", LegacyBookingVarianceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyBookingVarianceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyBookingVarianceStatus next)
    {
        switch (Status)
        {
            case LegacyBookingVarianceStatus.Open:
                return next == LegacyBookingVarianceStatus.InReview;
            case LegacyBookingVarianceStatus.InReview:
                return next == LegacyBookingVarianceStatus.Resolved || next == LegacyBookingVarianceStatus.Archived;
            case LegacyBookingVarianceStatus.Resolved:
                return next == LegacyBookingVarianceStatus.Archived;
            case LegacyBookingVarianceStatus.Reopened:
                return next == LegacyBookingVarianceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LegacyBookingVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy booking variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)StartsAt - (double)BatchSize, 2);
    }

    public LegacyBookingVariance Copy()
    {
        return new LegacyBookingVariance(Code, Name, BatchSize, StartsAt, Label, Visible, Grade, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
