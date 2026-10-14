namespace Acme.Workforce.Orgdesign.CostModel.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model snapshot is in its life cycle.</summary>
public enum CostModelSnapshotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a cost model snapshot and its current status.</summary>
public sealed class CostModelSnapshot(string code, string title, int revision, string label, string location, int createdAt, CostModelSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public int CreatedAt { get; set; } = createdAt;
    public CostModelSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a cost model snapshot in status requested.</summary>
    public static CostModelSnapshot NewCostModelSnapshot(string code, string title)
    {
        return new CostModelSnapshot(code, title, 0, "", "", 0, CostModelSnapshotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelSnapshotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelSnapshotStatus next)
    {
        switch (Status)
        {
            case CostModelSnapshotStatus.Requested:
                return next == CostModelSnapshotStatus.Expired;
            case CostModelSnapshotStatus.Granted:
                return next == CostModelSnapshotStatus.Revoked || next == CostModelSnapshotStatus.Expired;
            case CostModelSnapshotStatus.Revoked:
                return next == CostModelSnapshotStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CostModelSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"cost model snapshot"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Revision + (double)CreatedAt, 2);
    }

    public CostModelSnapshot Copy()
    {
        return new CostModelSnapshot(Code, Title, Revision, Label, Location, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
