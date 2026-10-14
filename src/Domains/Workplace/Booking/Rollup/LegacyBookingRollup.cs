namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy booking rollup is in its life cycle.</summary>
public enum LegacyBookingRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A legacy booking rollup. Its code is unique within a tenant.</summary>
public sealed class LegacyBookingRollup(string code, string title, int createdAt, string location, double minimum, string badge, double score, LegacyBookingRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Location { get; set; } = location;
    public double Minimum { get; set; } = minimum;
    public string Badge { get; set; } = badge;
    public double Score { get; set; } = score;
    public LegacyBookingRollupStatus Status { get; set; } = status;

    /// <summary>Starts a legacy booking rollup in status proposed.</summary>
    public static LegacyBookingRollup NewLegacyBookingRollup(string code, string title)
    {
        return new LegacyBookingRollup(code, title, 0, "", 0.0, "", 0.0, LegacyBookingRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyBookingRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyBookingRollupStatus next)
    {
        switch (Status)
        {
            case LegacyBookingRollupStatus.Proposed:
                return next == LegacyBookingRollupStatus.Rejected || next == LegacyBookingRollupStatus.Withdrawn;
            case LegacyBookingRollupStatus.Approved:
                switch (next)
                {
                    case LegacyBookingRollupStatus.Rejected:
                    case LegacyBookingRollupStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacyBookingRollupStatus.Rejected:
                return next == LegacyBookingRollupStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LegacyBookingRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy booking rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"legacy booking rollup"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)CreatedAt * Minimum, 2);
    }

    public LegacyBookingRollup Copy()
    {
        return new LegacyBookingRollup(Code, Title, CreatedAt, Location, Minimum, Badge, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
