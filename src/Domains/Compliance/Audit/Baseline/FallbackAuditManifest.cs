namespace Acme.Workforce.Compliance.Audit.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback audit manifest is in its life cycle.</summary>
public enum FallbackAuditManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A fallback audit manifest as the audit team keeps it.</summary>
public sealed class FallbackAuditManifest(string code, string label, string locale, int createdAt, bool pinned, string grade, string team, string source, string unit, int expiresAt, string channel, FallbackAuditManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public int CreatedAt { get; set; } = createdAt;
    public bool Pinned { get; set; } = pinned;
    public string Grade { get; set; } = grade;
    public string Team { get; set; } = team;
    public string Source { get; set; } = source;
    public string Unit { get; set; } = unit;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Channel { get; set; } = channel;
    public FallbackAuditManifestStatus Status { get; set; } = status;

    /// <summary>Starts a fallback audit manifest in status requested.</summary>
    public static FallbackAuditManifest NewFallbackAuditManifest(string code, string label)
    {
        return new FallbackAuditManifest(
            code,
            label,
            "",
            0,
            false,
            "",
            "",
            "",
            "",
            0,
            "",
            FallbackAuditManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAuditManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAuditManifestStatus next)
    {
        switch (Status)
        {
            case FallbackAuditManifestStatus.Requested:
                return next == FallbackAuditManifestStatus.Revoked;
            case FallbackAuditManifestStatus.Granted:
                switch (next)
                {
                    case FallbackAuditManifestStatus.Expired:
                        return true;
                }
                return false;
            case FallbackAuditManifestStatus.Revoked:
                switch (next)
                {
                    case FallbackAuditManifestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackAuditManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback audit manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)CreatedAt - (double)ExpiresAt, 2);
    }

    public FallbackAuditManifest Copy()
    {
        return new FallbackAuditManifest(
            Code,
            Label,
            Locale,
            CreatedAt,
            Pinned,
            Grade,
            Team,
            Source,
            Unit,
            ExpiresAt,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
