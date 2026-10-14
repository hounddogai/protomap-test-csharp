namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered holiday snapshot is in its life cycle.</summary>
public enum TieredHolidaySnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a tiered holiday snapshot and its current status.</summary>
public sealed class TieredHolidaySnapshot(string code, string name, bool active, bool archived, string employeeId, string label, string notes, int endsAt, string location, double score, TieredHolidaySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public bool Archived { get; set; } = archived;
    public string EmployeeId { get; set; } = employeeId;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public int EndsAt { get; set; } = endsAt;
    public string Location { get; set; } = location;
    public double Score { get; set; } = score;
    public TieredHolidaySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a tiered holiday snapshot in status idle.</summary>
    public static TieredHolidaySnapshot NewTieredHolidaySnapshot(string code, string name)
    {
        return new TieredHolidaySnapshot(
            code,
            name,
            false,
            false,
            "",
            "",
            "",
            0,
            "",
            0.0,
            TieredHolidaySnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredHolidaySnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredHolidaySnapshotStatus next)
    {
        switch (Status)
        {
            case TieredHolidaySnapshotStatus.Idle:
                switch (next)
                {
                    case TieredHolidaySnapshotStatus.Warming:
                    case TieredHolidaySnapshotStatus.Ready:
                        return true;
                }
                return false;
            case TieredHolidaySnapshotStatus.Warming:
                return next == TieredHolidaySnapshotStatus.Stopped;
            case TieredHolidaySnapshotStatus.Ready:
                return next == TieredHolidaySnapshotStatus.Draining || next == TieredHolidaySnapshotStatus.Stopped;
            case TieredHolidaySnapshotStatus.Draining:
                switch (next)
                {
                    case TieredHolidaySnapshotStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredHolidaySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered holiday snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double NetTotal()
    {
        return Math.Round((double)EndsAt * Score, 2);
    }

    public TieredHolidaySnapshot Copy()
    {
        return new TieredHolidaySnapshot(
            Code,
            Name,
            Active,
            Archived,
            EmployeeId,
            Label,
            Notes,
            EndsAt,
            Location,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
