namespace Acme.Workforce.Talent.CareerPath.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient career path schedule is in its life cycle.</summary>
public enum LenientCareerPathScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One lenient career path schedule, with what the planners need to act on it.</summary>
public sealed class LenientCareerPathSchedule(string code, string name, string ownerId, string badge, int priority, double margin, LenientCareerPathScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public string Badge { get; set; } = badge;
    public int Priority { get; set; } = priority;
    public double Margin { get; set; } = margin;
    public LenientCareerPathScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient career path schedule in status planned.</summary>
    public static LenientCareerPathSchedule NewLenientCareerPathSchedule(string code, string name)
    {
        return new LenientCareerPathSchedule(code, name, "", "", 0, 0.0, LenientCareerPathScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientCareerPathScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientCareerPathScheduleStatus next)
    {
        switch (Status)
        {
            case LenientCareerPathScheduleStatus.Planned:
                return next == LenientCareerPathScheduleStatus.Scheduled || next == LenientCareerPathScheduleStatus.Cancelled;
            case LenientCareerPathScheduleStatus.Scheduled:
                return next == LenientCareerPathScheduleStatus.Completed;
            case LenientCareerPathScheduleStatus.InProgress:
                return next == LenientCareerPathScheduleStatus.Cancelled;
            case LenientCareerPathScheduleStatus.Completed:
                return next == LenientCareerPathScheduleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientCareerPathScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient career path schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Priority + Margin, 2);
    }

    public LenientCareerPathSchedule Copy()
    {
        return new LenientCareerPathSchedule(Code, Name, OwnerId, Badge, Priority, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
