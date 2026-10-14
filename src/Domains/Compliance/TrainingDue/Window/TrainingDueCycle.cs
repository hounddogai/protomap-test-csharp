namespace Acme.Workforce.Compliance.TrainingDue.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due cycle is in its life cycle.</summary>
public enum TrainingDueCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A training due cycle. Its code is unique within a tenant.</summary>
public sealed class TrainingDueCycle(string code, string title, int createdAt, string name, int revision, TrainingDueCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public TrainingDueCycleStatus Status { get; set; } = status;

    /// <summary>Starts a training due cycle in status planned.</summary>
    public static TrainingDueCycle NewTrainingDueCycle(string code, string title)
    {
        return new TrainingDueCycle(code, title, 0, "", 0, TrainingDueCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueCycleStatus next)
    {
        switch (Status)
        {
            case TrainingDueCycleStatus.Planned:
                return next == TrainingDueCycleStatus.Completed || next == TrainingDueCycleStatus.Cancelled;
            case TrainingDueCycleStatus.Scheduled:
                return next == TrainingDueCycleStatus.Completed;
            case TrainingDueCycleStatus.InProgress:
                return next == TrainingDueCycleStatus.Completed || next == TrainingDueCycleStatus.Cancelled;
            case TrainingDueCycleStatus.Completed:
                return next == TrainingDueCycleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TrainingDueCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"training due cycle"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Revision * (double)CreatedAt, 2);
    }

    public TrainingDueCycle Copy()
    {
        return new TrainingDueCycle(Code, Title, CreatedAt, Name, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
