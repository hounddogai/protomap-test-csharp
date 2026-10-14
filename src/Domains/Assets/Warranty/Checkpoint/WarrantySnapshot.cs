namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty snapshot is in its life cycle.</summary>
public enum WarrantySnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A warranty snapshot as the warranty team keeps it.</summary>
public sealed class WarrantySnapshot(string code, string label, double budget, bool active, string managerId, double amount, double rating, WarrantySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public bool Active { get; set; } = active;
    public string ManagerId { get; set; } = managerId;
    public double Amount { get; set; } = amount;
    public double Rating { get; set; } = rating;
    public WarrantySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a warranty snapshot in status idle.</summary>
    public static WarrantySnapshot NewWarrantySnapshot(string code, string label)
    {
        return new WarrantySnapshot(code, label, 0.0, false, "", 0.0, 0.0, WarrantySnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantySnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantySnapshotStatus next)
    {
        switch (Status)
        {
            case WarrantySnapshotStatus.Idle:
                return next == WarrantySnapshotStatus.Warming || next == WarrantySnapshotStatus.Draining;
            case WarrantySnapshotStatus.Warming:
                return next == WarrantySnapshotStatus.Stopped;
            case WarrantySnapshotStatus.Ready:
                switch (next)
                {
                    case WarrantySnapshotStatus.Draining:
                        return true;
                }
                return false;
            case WarrantySnapshotStatus.Draining:
                return next == WarrantySnapshotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(WarrantySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Rating * Budget, 2);
    }

    public WarrantySnapshot Copy()
    {
        return new WarrantySnapshot(Code, Label, Budget, Active, ManagerId, Amount, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
