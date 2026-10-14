namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool snapshot is in its life cycle.</summary>
public enum BonusPoolSnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One bonus pool snapshot, with what the planners need to act on it.</summary>
public sealed class BonusPoolSnapshot(string code, string name, double rate, bool visible, int leaveDays, int quantity, string managerId, string badge, int tenureMonths, BonusPoolSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public bool Visible { get; set; } = visible;
    public int LeaveDays { get; set; } = leaveDays;
    public int Quantity { get; set; } = quantity;
    public string ManagerId { get; set; } = managerId;
    public string Badge { get; set; } = badge;
    public int TenureMonths { get; set; } = tenureMonths;
    public BonusPoolSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool snapshot in status new.</summary>
    public static BonusPoolSnapshot NewBonusPoolSnapshot(string code, string name)
    {
        return new BonusPoolSnapshot(code, name, 0.0, false, 0, 0, "", "", 0, BonusPoolSnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolSnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolSnapshotStatus next)
    {
        switch (Status)
        {
            case BonusPoolSnapshotStatus.New:
                switch (next)
                {
                    case BonusPoolSnapshotStatus.Queued:
                    case BonusPoolSnapshotStatus.Done:
                        return true;
                }
                return false;
            case BonusPoolSnapshotStatus.Queued:
                return next == BonusPoolSnapshotStatus.Running || next == BonusPoolSnapshotStatus.Done;
            case BonusPoolSnapshotStatus.Running:
                switch (next)
                {
                    case BonusPoolSnapshotStatus.Done:
                    case BonusPoolSnapshotStatus.Failed:
                        return true;
                }
                return false;
            case BonusPoolSnapshotStatus.Done:
                switch (next)
                {
                    case BonusPoolSnapshotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)LeaveDays * (double)TenureMonths, 2);
    }

    public BonusPoolSnapshot Copy()
    {
        return new BonusPoolSnapshot(
            Code,
            Name,
            Rate,
            Visible,
            LeaveDays,
            Quantity,
            ManagerId,
            Badge,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
