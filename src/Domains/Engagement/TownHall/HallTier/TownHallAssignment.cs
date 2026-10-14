namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall assignment is in its life cycle.</summary>
public enum TownHallAssignmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a town hall assignment and its current status.</summary>
public sealed class TownHallAssignment(string code, string title, int limit, int windowDays, string country, bool active, int attempts, TownHallAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public int WindowDays { get; set; } = windowDays;
    public string Country { get; set; } = country;
    public bool Active { get; set; } = active;
    public int Attempts { get; set; } = attempts;
    public TownHallAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a town hall assignment in status planned.</summary>
    public static TownHallAssignment NewTownHallAssignment(string code, string title)
    {
        return new TownHallAssignment(code, title, 0, 0, "", false, 0, TownHallAssignmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallAssignmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallAssignmentStatus next)
    {
        switch (Status)
        {
            case TownHallAssignmentStatus.Planned:
                switch (next)
                {
                    case TownHallAssignmentStatus.Scheduled:
                    case TownHallAssignmentStatus.InProgress:
                        return true;
                }
                return false;
            case TownHallAssignmentStatus.Scheduled:
                return next == TownHallAssignmentStatus.Completed;
            case TownHallAssignmentStatus.InProgress:
                switch (next)
                {
                    case TownHallAssignmentStatus.Completed:
                    case TownHallAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
            case TownHallAssignmentStatus.Completed:
                switch (next)
                {
                    case TownHallAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)WindowDays * (double)Limit, 2);
    }

    public TownHallAssignment Copy()
    {
        return new TownHallAssignment(Code, Title, Limit, WindowDays, Country, Active, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
