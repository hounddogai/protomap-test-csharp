namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request slot is in its life cycle.</summary>
public enum LeaveRequestSlotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A leave request slot. Its code is unique within a tenant.</summary>
public sealed class LeaveRequestSlot(string code, string label, string name, bool archived, bool approved, string costCenter, int startsAt, int attempts, double fte, int quantity, LeaveRequestSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public bool Approved { get; set; } = approved;
    public string CostCenter { get; set; } = costCenter;
    public int StartsAt { get; set; } = startsAt;
    public int Attempts { get; set; } = attempts;
    public double Fte { get; set; } = fte;
    public int Quantity { get; set; } = quantity;
    public LeaveRequestSlotStatus Status { get; set; } = status;

    /// <summary>Starts a leave request slot in status idle.</summary>
    public static LeaveRequestSlot NewLeaveRequestSlot(string code, string label)
    {
        return new LeaveRequestSlot(code, label, "", false, false, "", 0, 0, 0.0, 0, LeaveRequestSlotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestSlotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestSlotStatus next)
    {
        switch (Status)
        {
            case LeaveRequestSlotStatus.Idle:
                return next == LeaveRequestSlotStatus.Draining;
            case LeaveRequestSlotStatus.Warming:
                return next == LeaveRequestSlotStatus.Draining;
            case LeaveRequestSlotStatus.Ready:
                return next == LeaveRequestSlotStatus.Stopped;
            case LeaveRequestSlotStatus.Draining:
                return next == LeaveRequestSlotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LeaveRequestSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)StartsAt + (double)Quantity, 2);
    }

    public LeaveRequestSlot Copy()
    {
        return new LeaveRequestSlot(
            Code,
            Label,
            Name,
            Archived,
            Approved,
            CostCenter,
            StartsAt,
            Attempts,
            Fte,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
