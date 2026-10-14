namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal booking segment is in its life cycle.</summary>
public enum SeasonalBookingSegmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a seasonal booking segment and its current status.</summary>
public sealed class SeasonalBookingSegment(string code, string name, bool archived, string skill, bool active, int capacity, int graceDays, int updatedAt, bool visible, int expiresAt, int retries, SeasonalBookingSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Skill { get; set; } = skill;
    public bool Active { get; set; } = active;
    public int Capacity { get; set; } = capacity;
    public int GraceDays { get; set; } = graceDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Visible { get; set; } = visible;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Retries { get; set; } = retries;
    public SeasonalBookingSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal booking segment in status new.</summary>
    public static SeasonalBookingSegment NewSeasonalBookingSegment(string code, string name)
    {
        return new SeasonalBookingSegment(
            code,
            name,
            false,
            "",
            false,
            0,
            0,
            0,
            false,
            0,
            0,
            SeasonalBookingSegmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalBookingSegmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalBookingSegmentStatus next)
    {
        switch (Status)
        {
            case SeasonalBookingSegmentStatus.New:
                switch (next)
                {
                    case SeasonalBookingSegmentStatus.Queued:
                        return true;
                }
                return false;
            case SeasonalBookingSegmentStatus.Queued:
                return next == SeasonalBookingSegmentStatus.Failed;
            case SeasonalBookingSegmentStatus.Running:
                switch (next)
                {
                    case SeasonalBookingSegmentStatus.Done:
                    case SeasonalBookingSegmentStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalBookingSegmentStatus.Done:
                switch (next)
                {
                    case SeasonalBookingSegmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalBookingSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal booking segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetValue()
    {
        return Math.Round((double)UpdatedAt * (double)ExpiresAt, 2);
    }

    public SeasonalBookingSegment Copy()
    {
        return new SeasonalBookingSegment(
            Code,
            Name,
            Archived,
            Skill,
            Active,
            Capacity,
            GraceDays,
            UpdatedAt,
            Visible,
            ExpiresAt,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
