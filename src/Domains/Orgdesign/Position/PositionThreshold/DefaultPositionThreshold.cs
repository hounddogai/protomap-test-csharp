namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default position threshold is in its life cycle.</summary>
public enum DefaultPositionThresholdStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One default position threshold, with what the planners need to act on it.</summary>
public sealed class DefaultPositionThreshold(string code, string title, int windowDays, string managerId, double rating, DefaultPositionThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public string ManagerId { get; set; } = managerId;
    public double Rating { get; set; } = rating;
    public DefaultPositionThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a default position threshold in status planned.</summary>
    public static DefaultPositionThreshold NewDefaultPositionThreshold(string code, string title)
    {
        return new DefaultPositionThreshold(code, title, 0, "", 0.0, DefaultPositionThresholdStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPositionThresholdStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPositionThresholdStatus next)
    {
        switch (Status)
        {
            case DefaultPositionThresholdStatus.Planned:
                return next == DefaultPositionThresholdStatus.Cancelled;
            case DefaultPositionThresholdStatus.Scheduled:
                return next == DefaultPositionThresholdStatus.Completed || next == DefaultPositionThresholdStatus.Cancelled;
            case DefaultPositionThresholdStatus.InProgress:
                switch (next)
                {
                    case DefaultPositionThresholdStatus.Completed:
                        return true;
                }
                return false;
            case DefaultPositionThresholdStatus.Completed:
                return next == DefaultPositionThresholdStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DefaultPositionThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default position threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Rating - (double)WindowDays, 2);
    }

    public DefaultPositionThreshold Copy()
    {
        return new DefaultPositionThreshold(Code, Title, WindowDays, ManagerId, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
