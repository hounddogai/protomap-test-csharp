namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall checkpoint is in its life cycle.</summary>
public enum TownHallCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One town hall checkpoint, with what the planners need to act on it.</summary>
public sealed class TownHallCheckpoint(string code, string title, double floorValue, double margin, string name, string region, int leaveDays, string segment, TownHallCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public double Margin { get; set; } = margin;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int LeaveDays { get; set; } = leaveDays;
    public string Segment { get; set; } = segment;
    public TownHallCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a town hall checkpoint in status planned.</summary>
    public static TownHallCheckpoint NewTownHallCheckpoint(string code, string title)
    {
        return new TownHallCheckpoint(code, title, 0.0, 0.0, "", "", 0, "", TownHallCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallCheckpointStatus next)
    {
        switch (Status)
        {
            case TownHallCheckpointStatus.Planned:
                switch (next)
                {
                    case TownHallCheckpointStatus.Scheduled:
                        return true;
                }
                return false;
            case TownHallCheckpointStatus.Scheduled:
                return next == TownHallCheckpointStatus.Completed;
            case TownHallCheckpointStatus.InProgress:
                switch (next)
                {
                    case TownHallCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
            case TownHallCheckpointStatus.Completed:
                return next == TownHallCheckpointStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TownHallCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"town hall checkpoint"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Margin * (double)LeaveDays, 2);
    }

    public TownHallCheckpoint Copy()
    {
        return new TownHallCheckpoint(Code, Title, FloorValue, Margin, Name, Region, LeaveDays, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
