namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback pay band snapshot is in its life cycle.</summary>
public enum FallbackPayBandSnapshotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A fallback pay band snapshot as the pay band team keeps it.</summary>
public sealed class FallbackPayBandSnapshot(string code, string title, bool archived, int version, string managerId, string currency, double rate, double weight, FallbackPayBandSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public int Version { get; set; } = version;
    public string ManagerId { get; set; } = managerId;
    public string Currency { get; set; } = currency;
    public double Rate { get; set; } = rate;
    public double Weight { get; set; } = weight;
    public FallbackPayBandSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a fallback pay band snapshot in status proposed.</summary>
    public static FallbackPayBandSnapshot NewFallbackPayBandSnapshot(string code, string title)
    {
        return new FallbackPayBandSnapshot(
            code,
            title,
            false,
            0,
            "",
            "",
            0.0,
            0.0,
            FallbackPayBandSnapshotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPayBandSnapshotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPayBandSnapshotStatus next)
    {
        switch (Status)
        {
            case FallbackPayBandSnapshotStatus.Proposed:
                return next == FallbackPayBandSnapshotStatus.Withdrawn;
            case FallbackPayBandSnapshotStatus.Approved:
                return next == FallbackPayBandSnapshotStatus.Rejected || next == FallbackPayBandSnapshotStatus.Withdrawn;
            case FallbackPayBandSnapshotStatus.Rejected:
                return next == FallbackPayBandSnapshotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FallbackPayBandSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback pay band snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"fallback pay band snapshot"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Rate + Weight, 2);
    }

    public FallbackPayBandSnapshot Copy()
    {
        return new FallbackPayBandSnapshot(
            Code,
            Title,
            Archived,
            Version,
            ManagerId,
            Currency,
            Rate,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
