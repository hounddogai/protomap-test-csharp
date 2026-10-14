namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global town hall checkpoint is in its life cycle.</summary>
public enum GlobalTownHallCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One global town hall checkpoint, with what the planners need to act on it.</summary>
public sealed class GlobalTownHallCheckpoint(string code, string label, double hours, string badge, int retries, double threshold, string grade, string team, double floorValue, GlobalTownHallCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public string Badge { get; set; } = badge;
    public int Retries { get; set; } = retries;
    public double Threshold { get; set; } = threshold;
    public string Grade { get; set; } = grade;
    public string Team { get; set; } = team;
    public double FloorValue { get; set; } = floorValue;
    public GlobalTownHallCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a global town hall checkpoint in status planned.</summary>
    public static GlobalTownHallCheckpoint NewGlobalTownHallCheckpoint(string code, string label)
    {
        return new GlobalTownHallCheckpoint(
            code,
            label,
            0.0,
            "",
            0,
            0.0,
            "",
            "",
            0.0,
            GlobalTownHallCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalTownHallCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalTownHallCheckpointStatus next)
    {
        switch (Status)
        {
            case GlobalTownHallCheckpointStatus.Planned:
                return next == GlobalTownHallCheckpointStatus.InProgress || next == GlobalTownHallCheckpointStatus.Completed;
            case GlobalTownHallCheckpointStatus.Scheduled:
                switch (next)
                {
                    case GlobalTownHallCheckpointStatus.Completed:
                        return true;
                }
                return false;
            case GlobalTownHallCheckpointStatus.InProgress:
                return next == GlobalTownHallCheckpointStatus.Completed;
            case GlobalTownHallCheckpointStatus.Completed:
                switch (next)
                {
                    case GlobalTownHallCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalTownHallCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global town hall checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"global town hall checkpoint"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(FloorValue * Hours, 2);
    }

    public GlobalTownHallCheckpoint Copy()
    {
        return new GlobalTownHallCheckpoint(
            Code,
            Label,
            Hours,
            Badge,
            Retries,
            Threshold,
            Grade,
            Team,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
