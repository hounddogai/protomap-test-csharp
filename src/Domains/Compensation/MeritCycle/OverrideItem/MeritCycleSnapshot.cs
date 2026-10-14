namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle snapshot is in its life cycle.</summary>
public enum MeritCycleSnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A merit cycle snapshot as the merit cycle team keeps it.</summary>
public sealed class MeritCycleSnapshot(string code, string label, int tenureMonths, int version, string region, int capacity, int createdAt, MeritCycleSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Version { get; set; } = version;
    public string Region { get; set; } = region;
    public int Capacity { get; set; } = capacity;
    public int CreatedAt { get; set; } = createdAt;
    public MeritCycleSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle snapshot in status idle.</summary>
    public static MeritCycleSnapshot NewMeritCycleSnapshot(string code, string label)
    {
        return new MeritCycleSnapshot(code, label, 0, 0, "", 0, 0, MeritCycleSnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleSnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleSnapshotStatus next)
    {
        switch (Status)
        {
            case MeritCycleSnapshotStatus.Idle:
                switch (next)
                {
                    case MeritCycleSnapshotStatus.Ready:
                        return true;
                }
                return false;
            case MeritCycleSnapshotStatus.Warming:
                return next == MeritCycleSnapshotStatus.Stopped;
            case MeritCycleSnapshotStatus.Ready:
                return next == MeritCycleSnapshotStatus.Draining;
            case MeritCycleSnapshotStatus.Draining:
                return next == MeritCycleSnapshotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(MeritCycleSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Version * (double)TenureMonths, 2);
    }

    public MeritCycleSnapshot Copy()
    {
        return new MeritCycleSnapshot(Code, Label, TenureMonths, Version, Region, Capacity, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
