namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict equity grant milestone is in its life cycle.</summary>
public enum StrictEquityGrantMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a strict equity grant milestone and its current status.</summary>
public sealed class StrictEquityGrantMilestone(string code, string title, bool approved, int durationMinutes, int createdAt, string region, StrictEquityGrantMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int CreatedAt { get; set; } = createdAt;
    public string Region { get; set; } = region;
    public StrictEquityGrantMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a strict equity grant milestone in status proposed.</summary>
    public static StrictEquityGrantMilestone NewStrictEquityGrantMilestone(string code, string title)
    {
        return new StrictEquityGrantMilestone(
            code,
            title,
            false,
            0,
            0,
            "",
            StrictEquityGrantMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictEquityGrantMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictEquityGrantMilestoneStatus next)
    {
        switch (Status)
        {
            case StrictEquityGrantMilestoneStatus.Proposed:
                switch (next)
                {
                    case StrictEquityGrantMilestoneStatus.Approved:
                        return true;
                }
                return false;
            case StrictEquityGrantMilestoneStatus.Approved:
                return next == StrictEquityGrantMilestoneStatus.Rejected || next == StrictEquityGrantMilestoneStatus.Withdrawn;
            case StrictEquityGrantMilestoneStatus.Rejected:
                return next == StrictEquityGrantMilestoneStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictEquityGrantMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict equity grant milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"strict equity grant milestone"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)DurationMinutes * (double)CreatedAt, 2);
    }

    public StrictEquityGrantMilestone Copy()
    {
        return new StrictEquityGrantMilestone(Code, Title, Approved, DurationMinutes, CreatedAt, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
