namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped booking preference is in its life cycle.</summary>
public enum CappedBookingPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A capped booking preference. Its code is unique within a tenant.</summary>
public sealed class CappedBookingPreference(string code, string label, int batchSize, bool approved, string unit, string checksum, string description, bool visible, int priority, string team, CappedBookingPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public bool Approved { get; set; } = approved;
    public string Unit { get; set; } = unit;
    public string Checksum { get; set; } = checksum;
    public string Description { get; set; } = description;
    public bool Visible { get; set; } = visible;
    public int Priority { get; set; } = priority;
    public string Team { get; set; } = team;
    public CappedBookingPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a capped booking preference in status open.</summary>
    public static CappedBookingPreference NewCappedBookingPreference(string code, string label)
    {
        return new CappedBookingPreference(
            code,
            label,
            0,
            false,
            "",
            "",
            "",
            false,
            0,
            "",
            CappedBookingPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedBookingPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedBookingPreferenceStatus next)
    {
        switch (Status)
        {
            case CappedBookingPreferenceStatus.Open:
                return next == CappedBookingPreferenceStatus.InReview || next == CappedBookingPreferenceStatus.Archived;
            case CappedBookingPreferenceStatus.InReview:
                return next == CappedBookingPreferenceStatus.Reopened;
            case CappedBookingPreferenceStatus.Resolved:
                return next == CappedBookingPreferenceStatus.Reopened || next == CappedBookingPreferenceStatus.Archived;
            case CappedBookingPreferenceStatus.Reopened:
                return next == CappedBookingPreferenceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(CappedBookingPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped booking preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped booking preference"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Priority + (double)BatchSize, 2);
    }

    public CappedBookingPreference Copy()
    {
        return new CappedBookingPreference(
            Code,
            Label,
            BatchSize,
            Approved,
            Unit,
            Checksum,
            Description,
            Visible,
            Priority,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
