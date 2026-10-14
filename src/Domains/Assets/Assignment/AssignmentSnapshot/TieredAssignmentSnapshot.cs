namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered assignment snapshot is in its life cycle.</summary>
public enum TieredAssignmentSnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A tiered assignment snapshot as the assignment team keeps it.</summary>
public sealed class TieredAssignmentSnapshot(string code, string name, string timezone, int graceDays, int retries, int updatedAt, string externalRef, string department, int durationMinutes, bool archived, TieredAssignmentSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public int GraceDays { get; set; } = graceDays;
    public int Retries { get; set; } = retries;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ExternalRef { get; set; } = externalRef;
    public string Department { get; set; } = department;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Archived { get; set; } = archived;
    public TieredAssignmentSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a tiered assignment snapshot in status idle.</summary>
    public static TieredAssignmentSnapshot NewTieredAssignmentSnapshot(string code, string name)
    {
        return new TieredAssignmentSnapshot(
            code,
            name,
            "",
            0,
            0,
            0,
            "",
            "",
            0,
            false,
            TieredAssignmentSnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredAssignmentSnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredAssignmentSnapshotStatus next)
    {
        switch (Status)
        {
            case TieredAssignmentSnapshotStatus.Idle:
                return next == TieredAssignmentSnapshotStatus.Draining;
            case TieredAssignmentSnapshotStatus.Warming:
                return next == TieredAssignmentSnapshotStatus.Draining || next == TieredAssignmentSnapshotStatus.Stopped;
            case TieredAssignmentSnapshotStatus.Ready:
                switch (next)
                {
                    case TieredAssignmentSnapshotStatus.Draining:
                        return true;
                }
                return false;
            case TieredAssignmentSnapshotStatus.Draining:
                return next == TieredAssignmentSnapshotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TieredAssignmentSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered assignment snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"tiered assignment snapshot"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)GraceDays * (double)DurationMinutes, 2);
    }

    public TieredAssignmentSnapshot Copy()
    {
        return new TieredAssignmentSnapshot(
            Code,
            Name,
            Timezone,
            GraceDays,
            Retries,
            UpdatedAt,
            ExternalRef,
            Department,
            DurationMinutes,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
