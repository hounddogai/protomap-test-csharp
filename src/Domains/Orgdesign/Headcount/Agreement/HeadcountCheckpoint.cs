namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount checkpoint is in its life cycle.</summary>
public enum HeadcountCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A headcount checkpoint as the headcount team keeps it.</summary>
public sealed class HeadcountCheckpoint(string code, string name, string category, string contactEmail, int durationMinutes, bool archived, HeadcountCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string ContactEmail { get; set; } = contactEmail;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Archived { get; set; } = archived;
    public HeadcountCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a headcount checkpoint in status idle.</summary>
    public static HeadcountCheckpoint NewHeadcountCheckpoint(string code, string name)
    {
        return new HeadcountCheckpoint(code, name, "", "", 0, false, HeadcountCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountCheckpointStatus next)
    {
        switch (Status)
        {
            case HeadcountCheckpointStatus.Idle:
                return next == HeadcountCheckpointStatus.Ready;
            case HeadcountCheckpointStatus.Warming:
                return next == HeadcountCheckpointStatus.Stopped;
            case HeadcountCheckpointStatus.Ready:
                switch (next)
                {
                    case HeadcountCheckpointStatus.Draining:
                    case HeadcountCheckpointStatus.Stopped:
                        return true;
                }
                return false;
            case HeadcountCheckpointStatus.Draining:
                switch (next)
                {
                    case HeadcountCheckpointStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public HeadcountCheckpoint Copy()
    {
        return new HeadcountCheckpoint(Code, Name, Category, ContactEmail, DurationMinutes, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
