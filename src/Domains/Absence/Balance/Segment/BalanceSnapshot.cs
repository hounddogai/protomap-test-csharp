namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance snapshot is in its life cycle.</summary>
public enum BalanceSnapshotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One balance snapshot, with what the planners need to act on it.</summary>
public sealed class BalanceSnapshot(string code, string label, string segment, int capacity, int version, double amount, string employeeId, int startsAt, BalanceSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public int Capacity { get; set; } = capacity;
    public int Version { get; set; } = version;
    public double Amount { get; set; } = amount;
    public string EmployeeId { get; set; } = employeeId;
    public int StartsAt { get; set; } = startsAt;
    public BalanceSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a balance snapshot in status proposed.</summary>
    public static BalanceSnapshot NewBalanceSnapshot(string code, string label)
    {
        return new BalanceSnapshot(code, label, "", 0, 0, 0.0, "", 0, BalanceSnapshotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceSnapshotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceSnapshotStatus next)
    {
        switch (Status)
        {
            case BalanceSnapshotStatus.Proposed:
                switch (next)
                {
                    case BalanceSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
            case BalanceSnapshotStatus.Approved:
                return next == BalanceSnapshotStatus.Rejected || next == BalanceSnapshotStatus.Withdrawn;
            case BalanceSnapshotStatus.Rejected:
                switch (next)
                {
                    case BalanceSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)StartsAt - (double)Capacity, 2);
    }

    public BalanceSnapshot Copy()
    {
        return new BalanceSnapshot(Code, Label, Segment, Capacity, Version, Amount, EmployeeId, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
