namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling retro pay assignment is in its life cycle.</summary>
public enum RollingRetroPayAssignmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One rolling retro pay assignment, with what the planners need to act on it.</summary>
public sealed class RollingRetroPayAssignment(string code, string title, string location, int revision, int durationMinutes, string locale, RollingRetroPayAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public int Revision { get; set; } = revision;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Locale { get; set; } = locale;
    public RollingRetroPayAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling retro pay assignment in status planned.</summary>
    public static RollingRetroPayAssignment NewRollingRetroPayAssignment(string code, string title)
    {
        return new RollingRetroPayAssignment(code, title, "", 0, 0, "", RollingRetroPayAssignmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRetroPayAssignmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRetroPayAssignmentStatus next)
    {
        switch (Status)
        {
            case RollingRetroPayAssignmentStatus.Planned:
                return next == RollingRetroPayAssignmentStatus.Scheduled || next == RollingRetroPayAssignmentStatus.Completed;
            case RollingRetroPayAssignmentStatus.Scheduled:
                return next == RollingRetroPayAssignmentStatus.Completed;
            case RollingRetroPayAssignmentStatus.InProgress:
                switch (next)
                {
                    case RollingRetroPayAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingRetroPayAssignmentStatus.Completed:
                return next == RollingRetroPayAssignmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RollingRetroPayAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling retro pay assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Revision - (double)DurationMinutes, 2);
    }

    public RollingRetroPayAssignment Copy()
    {
        return new RollingRetroPayAssignment(Code, Title, Location, Revision, DurationMinutes, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
