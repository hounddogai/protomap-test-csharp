namespace Acme.Workforce.Workplace.Booking.BookingPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual booking audit is in its life cycle.</summary>
public enum ManualBookingAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A manual booking audit as the booking team keeps it.</summary>
public sealed class ManualBookingAudit(string code, string title, int createdAt, string source, string name, int retries, int version, bool pinned, int revision, ManualBookingAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Source { get; set; } = source;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public int Version { get; set; } = version;
    public bool Pinned { get; set; } = pinned;
    public int Revision { get; set; } = revision;
    public ManualBookingAuditStatus Status { get; set; } = status;

    /// <summary>Starts a manual booking audit in status proposed.</summary>
    public static ManualBookingAudit NewManualBookingAudit(string code, string title)
    {
        return new ManualBookingAudit(code, title, 0, "", "", 0, 0, false, 0, ManualBookingAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBookingAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBookingAuditStatus next)
    {
        switch (Status)
        {
            case ManualBookingAuditStatus.Proposed:
                switch (next)
                {
                    case ManualBookingAuditStatus.Rejected:
                    case ManualBookingAuditStatus.Withdrawn:
                        return true;
                }
                return false;
            case ManualBookingAuditStatus.Approved:
                return next == ManualBookingAuditStatus.Rejected || next == ManualBookingAuditStatus.Withdrawn;
            case ManualBookingAuditStatus.Rejected:
                switch (next)
                {
                    case ManualBookingAuditStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualBookingAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual booking audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Retries + (double)Version, 2);
    }

    public ManualBookingAudit Copy()
    {
        return new ManualBookingAudit(
            Code,
            Title,
            CreatedAt,
            Source,
            Name,
            Retries,
            Version,
            Pinned,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
