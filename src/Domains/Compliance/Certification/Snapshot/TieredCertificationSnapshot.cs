namespace Acme.Workforce.Compliance.Certification.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered certification snapshot is in its life cycle.</summary>
public enum TieredCertificationSnapshotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered certification snapshot. Its code is unique within a tenant.</summary>
public sealed class TieredCertificationSnapshot(string code, string label, int capacity, int sequence, string employeeId, int limit, string jobCode, string source, double weight, TieredCertificationSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public int Sequence { get; set; } = sequence;
    public string EmployeeId { get; set; } = employeeId;
    public int Limit { get; set; } = limit;
    public string JobCode { get; set; } = jobCode;
    public string Source { get; set; } = source;
    public double Weight { get; set; } = weight;
    public TieredCertificationSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a tiered certification snapshot in status requested.</summary>
    public static TieredCertificationSnapshot NewTieredCertificationSnapshot(string code, string label)
    {
        return new TieredCertificationSnapshot(
            code,
            label,
            0,
            0,
            "",
            0,
            "",
            "",
            0.0,
            TieredCertificationSnapshotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredCertificationSnapshotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredCertificationSnapshotStatus next)
    {
        switch (Status)
        {
            case TieredCertificationSnapshotStatus.Requested:
                return next == TieredCertificationSnapshotStatus.Revoked;
            case TieredCertificationSnapshotStatus.Granted:
                return next == TieredCertificationSnapshotStatus.Revoked;
            case TieredCertificationSnapshotStatus.Revoked:
                return next == TieredCertificationSnapshotStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TieredCertificationSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered certification snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Capacity - Weight, 2);
    }

    public TieredCertificationSnapshot Copy()
    {
        return new TieredCertificationSnapshot(
            Code,
            Label,
            Capacity,
            Sequence,
            EmployeeId,
            Limit,
            JobCode,
            Source,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
