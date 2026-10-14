namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped refresh slot is in its life cycle.</summary>
public enum CappedRefreshSlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A capped refresh slot as the refresh team keeps it.</summary>
public sealed class CappedRefreshSlot(string code, string label, string employeeId, int retries, bool visible, int capacity, string ownerId, CappedRefreshSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public int Retries { get; set; } = retries;
    public bool Visible { get; set; } = visible;
    public int Capacity { get; set; } = capacity;
    public string OwnerId { get; set; } = ownerId;
    public CappedRefreshSlotStatus Status { get; set; } = status;

    /// <summary>Starts a capped refresh slot in status requested.</summary>
    public static CappedRefreshSlot NewCappedRefreshSlot(string code, string label)
    {
        return new CappedRefreshSlot(code, label, "", 0, false, 0, "", CappedRefreshSlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedRefreshSlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedRefreshSlotStatus next)
    {
        switch (Status)
        {
            case CappedRefreshSlotStatus.Requested:
                switch (next)
                {
                    case CappedRefreshSlotStatus.Granted:
                    case CappedRefreshSlotStatus.Revoked:
                        return true;
                }
                return false;
            case CappedRefreshSlotStatus.Granted:
                return next == CappedRefreshSlotStatus.Revoked || next == CappedRefreshSlotStatus.Expired;
            case CappedRefreshSlotStatus.Revoked:
                switch (next)
                {
                    case CappedRefreshSlotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedRefreshSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped refresh slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Retries + (double)Capacity, 2);
    }

    public CappedRefreshSlot Copy()
    {
        return new CappedRefreshSlot(Code, Label, EmployeeId, Retries, Visible, Capacity, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
