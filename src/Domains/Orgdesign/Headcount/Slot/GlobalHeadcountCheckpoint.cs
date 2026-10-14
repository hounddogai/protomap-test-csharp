namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global headcount checkpoint is in its life cycle.</summary>
public enum GlobalHeadcountCheckpointStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One global headcount checkpoint, with what the planners need to act on it.</summary>
public sealed class GlobalHeadcountCheckpoint(string code, string title, bool active, string skill, int leaveDays, string costCenter, GlobalHeadcountCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public string Skill { get; set; } = skill;
    public int LeaveDays { get; set; } = leaveDays;
    public string CostCenter { get; set; } = costCenter;
    public GlobalHeadcountCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a global headcount checkpoint in status open.</summary>
    public static GlobalHeadcountCheckpoint NewGlobalHeadcountCheckpoint(string code, string title)
    {
        return new GlobalHeadcountCheckpoint(code, title, false, "", 0, "", GlobalHeadcountCheckpointStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalHeadcountCheckpointStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalHeadcountCheckpointStatus next)
    {
        switch (Status)
        {
            case GlobalHeadcountCheckpointStatus.Open:
                switch (next)
                {
                    case GlobalHeadcountCheckpointStatus.Reopened:
                        return true;
                }
                return false;
            case GlobalHeadcountCheckpointStatus.InReview:
                switch (next)
                {
                    case GlobalHeadcountCheckpointStatus.Resolved:
                    case GlobalHeadcountCheckpointStatus.Archived:
                        return true;
                }
                return false;
            case GlobalHeadcountCheckpointStatus.Resolved:
                switch (next)
                {
                    case GlobalHeadcountCheckpointStatus.Reopened:
                    case GlobalHeadcountCheckpointStatus.Archived:
                        return true;
                }
                return false;
            case GlobalHeadcountCheckpointStatus.Reopened:
                return next == GlobalHeadcountCheckpointStatus.Archived;
        }
        return false;
    }

    public void MoveTo(GlobalHeadcountCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global headcount checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public GlobalHeadcountCheckpoint Copy()
    {
        return new GlobalHeadcountCheckpoint(Code, Title, Active, Skill, LeaveDays, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
