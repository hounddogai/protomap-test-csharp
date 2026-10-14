namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched leave request snapshot is in its life cycle.</summary>
public enum BatchedLeaveRequestSnapshotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A batched leave request snapshot. Its code is unique within a tenant.</summary>
public sealed class BatchedLeaveRequestSnapshot(string code, string name, bool active, string salaryBand, string ownerId, int quantity, BatchedLeaveRequestSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string SalaryBand { get; set; } = salaryBand;
    public string OwnerId { get; set; } = ownerId;
    public int Quantity { get; set; } = quantity;
    public BatchedLeaveRequestSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a batched leave request snapshot in status proposed.</summary>
    public static BatchedLeaveRequestSnapshot NewBatchedLeaveRequestSnapshot(string code, string name)
    {
        return new BatchedLeaveRequestSnapshot(
            code,
            name,
            false,
            "",
            "",
            0,
            BatchedLeaveRequestSnapshotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedLeaveRequestSnapshotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedLeaveRequestSnapshotStatus next)
    {
        switch (Status)
        {
            case BatchedLeaveRequestSnapshotStatus.Proposed:
                switch (next)
                {
                    case BatchedLeaveRequestSnapshotStatus.Rejected:
                        return true;
                }
                return false;
            case BatchedLeaveRequestSnapshotStatus.Approved:
                return next == BatchedLeaveRequestSnapshotStatus.Rejected || next == BatchedLeaveRequestSnapshotStatus.Withdrawn;
            case BatchedLeaveRequestSnapshotStatus.Rejected:
                switch (next)
                {
                    case BatchedLeaveRequestSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedLeaveRequestSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched leave request snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public BatchedLeaveRequestSnapshot Copy()
    {
        return new BatchedLeaveRequestSnapshot(Code, Name, Active, SalaryBand, OwnerId, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
