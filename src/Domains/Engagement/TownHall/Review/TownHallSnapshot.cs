namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall snapshot is in its life cycle.</summary>
public enum TownHallSnapshotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a town hall snapshot and its current status.</summary>
public sealed class TownHallSnapshot(string code, string label, string reviewer, int windowDays, int startsAt, string jobCode, string notes, string timezone, string badge, TownHallSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public int WindowDays { get; set; } = windowDays;
    public int StartsAt { get; set; } = startsAt;
    public string JobCode { get; set; } = jobCode;
    public string Notes { get; set; } = notes;
    public string Timezone { get; set; } = timezone;
    public string Badge { get; set; } = badge;
    public TownHallSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a town hall snapshot in status draft.</summary>
    public static TownHallSnapshot NewTownHallSnapshot(string code, string label)
    {
        return new TownHallSnapshot(code, label, "", 0, 0, "", "", "", "", TownHallSnapshotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallSnapshotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallSnapshotStatus next)
    {
        switch (Status)
        {
            case TownHallSnapshotStatus.Draft:
                switch (next)
                {
                    case TownHallSnapshotStatus.Suspended:
                        return true;
                }
                return false;
            case TownHallSnapshotStatus.Pending:
                return next == TownHallSnapshotStatus.Closed;
            case TownHallSnapshotStatus.Active:
                return next == TownHallSnapshotStatus.Suspended || next == TownHallSnapshotStatus.Closed;
            case TownHallSnapshotStatus.Suspended:
                switch (next)
                {
                    case TownHallSnapshotStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetVolume()
    {
        return Math.Round((double)StartsAt * (double)WindowDays, 2);
    }

    public TownHallSnapshot Copy()
    {
        return new TownHallSnapshot(
            Code,
            Label,
            Reviewer,
            WindowDays,
            StartsAt,
            JobCode,
            Notes,
            Timezone,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
