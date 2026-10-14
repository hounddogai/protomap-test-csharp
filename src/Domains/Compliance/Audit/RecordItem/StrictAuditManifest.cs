namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict audit manifest is in its life cycle.</summary>
public enum StrictAuditManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a strict audit manifest and its current status.</summary>
public sealed class StrictAuditManifest(string code, string title, bool active, bool archived, double minimum, StrictAuditManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public bool Archived { get; set; } = archived;
    public double Minimum { get; set; } = minimum;
    public StrictAuditManifestStatus Status { get; set; } = status;

    /// <summary>Starts a strict audit manifest in status requested.</summary>
    public static StrictAuditManifest NewStrictAuditManifest(string code, string title)
    {
        return new StrictAuditManifest(code, title, false, false, 0.0, StrictAuditManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAuditManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAuditManifestStatus next)
    {
        switch (Status)
        {
            case StrictAuditManifestStatus.Requested:
                return next == StrictAuditManifestStatus.Revoked || next == StrictAuditManifestStatus.Expired;
            case StrictAuditManifestStatus.Granted:
                return next == StrictAuditManifestStatus.Revoked || next == StrictAuditManifestStatus.Expired;
            case StrictAuditManifestStatus.Revoked:
                return next == StrictAuditManifestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StrictAuditManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict audit manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Minimum;
    }

    public StrictAuditManifest Copy()
    {
        return new StrictAuditManifest(Code, Title, Active, Archived, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
