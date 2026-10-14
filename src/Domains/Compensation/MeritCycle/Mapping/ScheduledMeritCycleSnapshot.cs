namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled merit cycle snapshot is in its life cycle.</summary>
public enum ScheduledMeritCycleSnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scheduled merit cycle snapshot. Its code is unique within a tenant.</summary>
public sealed class ScheduledMeritCycleSnapshot(string code, string name, string team, string checksum, double ceiling, string timezone, int leaveDays, int windowDays, double threshold, ScheduledMeritCycleSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public string Checksum { get; set; } = checksum;
    public double Ceiling { get; set; } = ceiling;
    public string Timezone { get; set; } = timezone;
    public int LeaveDays { get; set; } = leaveDays;
    public int WindowDays { get; set; } = windowDays;
    public double Threshold { get; set; } = threshold;
    public ScheduledMeritCycleSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled merit cycle snapshot in status open.</summary>
    public static ScheduledMeritCycleSnapshot NewScheduledMeritCycleSnapshot(string code, string name)
    {
        return new ScheduledMeritCycleSnapshot(
            code,
            name,
            "",
            "",
            0.0,
            "",
            0,
            0,
            0.0,
            ScheduledMeritCycleSnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledMeritCycleSnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledMeritCycleSnapshotStatus next)
    {
        switch (Status)
        {
            case ScheduledMeritCycleSnapshotStatus.Open:
                switch (next)
                {
                    case ScheduledMeritCycleSnapshotStatus.Reopened:
                        return true;
                }
                return false;
            case ScheduledMeritCycleSnapshotStatus.InReview:
                return next == ScheduledMeritCycleSnapshotStatus.Resolved || next == ScheduledMeritCycleSnapshotStatus.Archived;
            case ScheduledMeritCycleSnapshotStatus.Resolved:
                return next == ScheduledMeritCycleSnapshotStatus.Reopened;
            case ScheduledMeritCycleSnapshotStatus.Reopened:
                return next == ScheduledMeritCycleSnapshotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ScheduledMeritCycleSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled merit cycle snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled merit cycle snapshot"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Threshold * Ceiling, 2);
    }

    public ScheduledMeritCycleSnapshot Copy()
    {
        return new ScheduledMeritCycleSnapshot(
            Code,
            Name,
            Team,
            Checksum,
            Ceiling,
            Timezone,
            LeaveDays,
            WindowDays,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
