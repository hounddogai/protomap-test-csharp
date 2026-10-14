namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pool schedule is in its life cycle.</summary>
public enum StrictPoolScheduleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One strict pool schedule, with what the planners need to act on it.</summary>
public sealed class StrictPoolSchedule(string code, string name, int sequence, string source, string ownerId, bool approved, StrictPoolScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public string Source { get; set; } = source;
    public string OwnerId { get; set; } = ownerId;
    public bool Approved { get; set; } = approved;
    public StrictPoolScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a strict pool schedule in status new.</summary>
    public static StrictPoolSchedule NewStrictPoolSchedule(string code, string name)
    {
        return new StrictPoolSchedule(code, name, 0, "", "", false, StrictPoolScheduleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPoolScheduleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPoolScheduleStatus next)
    {
        switch (Status)
        {
            case StrictPoolScheduleStatus.New:
                return next == StrictPoolScheduleStatus.Failed;
            case StrictPoolScheduleStatus.Queued:
                switch (next)
                {
                    case StrictPoolScheduleStatus.Running:
                        return true;
                }
                return false;
            case StrictPoolScheduleStatus.Running:
                switch (next)
                {
                    case StrictPoolScheduleStatus.Done:
                    case StrictPoolScheduleStatus.Failed:
                        return true;
                }
                return false;
            case StrictPoolScheduleStatus.Done:
                return next == StrictPoolScheduleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(StrictPoolScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pool schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public StrictPoolSchedule Copy()
    {
        return new StrictPoolSchedule(Code, Name, Sequence, Source, OwnerId, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
