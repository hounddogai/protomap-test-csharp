namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered exception group is in its life cycle.</summary>
public enum TieredExceptionGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a tiered exception group and its current status.</summary>
public sealed class TieredExceptionGroup(string code, string label, string channel, string skill, int durationMinutes, int revision, string managerId, TieredExceptionGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Skill { get; set; } = skill;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Revision { get; set; } = revision;
    public string ManagerId { get; set; } = managerId;
    public TieredExceptionGroupStatus Status { get; set; } = status;

    /// <summary>Starts a tiered exception group in status planned.</summary>
    public static TieredExceptionGroup NewTieredExceptionGroup(string code, string label)
    {
        return new TieredExceptionGroup(code, label, "", "", 0, 0, "", TieredExceptionGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredExceptionGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredExceptionGroupStatus next)
    {
        switch (Status)
        {
            case TieredExceptionGroupStatus.Planned:
                return next == TieredExceptionGroupStatus.Completed;
            case TieredExceptionGroupStatus.Scheduled:
                switch (next)
                {
                    case TieredExceptionGroupStatus.Completed:
                        return true;
                }
                return false;
            case TieredExceptionGroupStatus.InProgress:
                switch (next)
                {
                    case TieredExceptionGroupStatus.Completed:
                        return true;
                }
                return false;
            case TieredExceptionGroupStatus.Completed:
                return next == TieredExceptionGroupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TieredExceptionGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered exception group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)DurationMinutes - (double)Revision, 2);
    }

    public TieredExceptionGroup Copy()
    {
        return new TieredExceptionGroup(Code, Label, Channel, Skill, DurationMinutes, Revision, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
