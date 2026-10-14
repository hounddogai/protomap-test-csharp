namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pay band manifest is in its life cycle.</summary>
public enum ScheduledPayBandManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a scheduled pay band manifest and its current status.</summary>
public sealed class ScheduledPayBandManifest(string code, string title, int expiresAt, int startsAt, string country, string checksum, bool archived, int windowDays, ScheduledPayBandManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public int StartsAt { get; set; } = startsAt;
    public string Country { get; set; } = country;
    public string Checksum { get; set; } = checksum;
    public bool Archived { get; set; } = archived;
    public int WindowDays { get; set; } = windowDays;
    public ScheduledPayBandManifestStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pay band manifest in status proposed.</summary>
    public static ScheduledPayBandManifest NewScheduledPayBandManifest(string code, string title)
    {
        return new ScheduledPayBandManifest(
            code,
            title,
            0,
            0,
            "",
            "",
            false,
            0,
            ScheduledPayBandManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayBandManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayBandManifestStatus next)
    {
        switch (Status)
        {
            case ScheduledPayBandManifestStatus.Proposed:
                return next == ScheduledPayBandManifestStatus.Approved;
            case ScheduledPayBandManifestStatus.Approved:
                return next == ScheduledPayBandManifestStatus.Rejected;
            case ScheduledPayBandManifestStatus.Rejected:
                switch (next)
                {
                    case ScheduledPayBandManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledPayBandManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pay band manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetTotal()
    {
        return Math.Round((double)WindowDays * (double)StartsAt, 2);
    }

    public ScheduledPayBandManifest Copy()
    {
        return new ScheduledPayBandManifest(
            Code,
            Title,
            ExpiresAt,
            StartsAt,
            Country,
            Checksum,
            Archived,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
