namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled bonus pool milestone is in its life cycle.</summary>
public enum ScheduledBonusPoolMilestoneStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scheduled bonus pool milestone. Its code is unique within a tenant.</summary>
public sealed class ScheduledBonusPoolMilestone(string code, string name, double hours, int createdAt, string costCenter, ScheduledBonusPoolMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public int CreatedAt { get; set; } = createdAt;
    public string CostCenter { get; set; } = costCenter;
    public ScheduledBonusPoolMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled bonus pool milestone in status open.</summary>
    public static ScheduledBonusPoolMilestone NewScheduledBonusPoolMilestone(string code, string name)
    {
        return new ScheduledBonusPoolMilestone(code, name, 0.0, 0, "", ScheduledBonusPoolMilestoneStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledBonusPoolMilestoneStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledBonusPoolMilestoneStatus next)
    {
        switch (Status)
        {
            case ScheduledBonusPoolMilestoneStatus.Open:
                switch (next)
                {
                    case ScheduledBonusPoolMilestoneStatus.InReview:
                    case ScheduledBonusPoolMilestoneStatus.Resolved:
                        return true;
                }
                return false;
            case ScheduledBonusPoolMilestoneStatus.InReview:
                switch (next)
                {
                    case ScheduledBonusPoolMilestoneStatus.Resolved:
                        return true;
                }
                return false;
            case ScheduledBonusPoolMilestoneStatus.Resolved:
                return next == ScheduledBonusPoolMilestoneStatus.Reopened || next == ScheduledBonusPoolMilestoneStatus.Archived;
            case ScheduledBonusPoolMilestoneStatus.Reopened:
                return next == ScheduledBonusPoolMilestoneStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ScheduledBonusPoolMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled bonus pool milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)CreatedAt - Hours, 2);
    }

    public ScheduledBonusPoolMilestone Copy()
    {
        return new ScheduledBonusPoolMilestone(Code, Name, Hours, CreatedAt, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
