namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request snapshot is in its life cycle.</summary>
public enum LeaveRequestSnapshotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A leave request snapshot as the leave request team keeps it.</summary>
public sealed class LeaveRequestSnapshot(string code, string label, string channel, int version, string notes, int windowDays, LeaveRequestSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public int Version { get; set; } = version;
    public string Notes { get; set; } = notes;
    public int WindowDays { get; set; } = windowDays;
    public LeaveRequestSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a leave request snapshot in status requested.</summary>
    public static LeaveRequestSnapshot NewLeaveRequestSnapshot(string code, string label)
    {
        return new LeaveRequestSnapshot(code, label, "", 0, "", 0, LeaveRequestSnapshotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestSnapshotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestSnapshotStatus next)
    {
        switch (Status)
        {
            case LeaveRequestSnapshotStatus.Requested:
                return next == LeaveRequestSnapshotStatus.Expired;
            case LeaveRequestSnapshotStatus.Granted:
                return next == LeaveRequestSnapshotStatus.Revoked || next == LeaveRequestSnapshotStatus.Expired;
            case LeaveRequestSnapshotStatus.Revoked:
                switch (next)
                {
                    case LeaveRequestSnapshotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)WindowDays + (double)Version, 2);
    }

    public LeaveRequestSnapshot Copy()
    {
        return new LeaveRequestSnapshot(Code, Label, Channel, Version, Notes, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
