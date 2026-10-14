namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal visitor checkpoint is in its life cycle.</summary>
public enum SeasonalVisitorCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One seasonal visitor checkpoint, with what the planners need to act on it.</summary>
public sealed class SeasonalVisitorCheckpoint(string code, string title, bool pinned, string managerId, int windowDays, string locale, SeasonalVisitorCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public string ManagerId { get; set; } = managerId;
    public int WindowDays { get; set; } = windowDays;
    public string Locale { get; set; } = locale;
    public SeasonalVisitorCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal visitor checkpoint in status idle.</summary>
    public static SeasonalVisitorCheckpoint NewSeasonalVisitorCheckpoint(string code, string title)
    {
        return new SeasonalVisitorCheckpoint(code, title, false, "", 0, "", SeasonalVisitorCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalVisitorCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalVisitorCheckpointStatus next)
    {
        switch (Status)
        {
            case SeasonalVisitorCheckpointStatus.Idle:
                return next == SeasonalVisitorCheckpointStatus.Stopped;
            case SeasonalVisitorCheckpointStatus.Warming:
                switch (next)
                {
                    case SeasonalVisitorCheckpointStatus.Ready:
                        return true;
                }
                return false;
            case SeasonalVisitorCheckpointStatus.Ready:
                return next == SeasonalVisitorCheckpointStatus.Stopped;
            case SeasonalVisitorCheckpointStatus.Draining:
                switch (next)
                {
                    case SeasonalVisitorCheckpointStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalVisitorCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal visitor checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public SeasonalVisitorCheckpoint Copy()
    {
        return new SeasonalVisitorCheckpoint(Code, Title, Pinned, ManagerId, WindowDays, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
