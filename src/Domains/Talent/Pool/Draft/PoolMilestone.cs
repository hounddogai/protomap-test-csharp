namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool milestone is in its life cycle.</summary>
public enum PoolMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pool milestone. Its code is unique within a tenant.</summary>
public sealed class PoolMilestone(string code, string title, string notes, double ratio, double rating, double fte, double floorValue, PoolMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public double Ratio { get; set; } = ratio;
    public double Rating { get; set; } = rating;
    public double Fte { get; set; } = fte;
    public double FloorValue { get; set; } = floorValue;
    public PoolMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a pool milestone in status proposed.</summary>
    public static PoolMilestone NewPoolMilestone(string code, string title)
    {
        return new PoolMilestone(code, title, "", 0.0, 0.0, 0.0, 0.0, PoolMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolMilestoneStatus next)
    {
        switch (Status)
        {
            case PoolMilestoneStatus.Proposed:
                switch (next)
                {
                    case PoolMilestoneStatus.Approved:
                    case PoolMilestoneStatus.Rejected:
                        return true;
                }
                return false;
            case PoolMilestoneStatus.Approved:
                return next == PoolMilestoneStatus.Withdrawn;
            case PoolMilestoneStatus.Rejected:
                return next == PoolMilestoneStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PoolMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pool milestone"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Fte - Ratio, 2);
    }

    public PoolMilestone Copy()
    {
        return new PoolMilestone(Code, Title, Notes, Ratio, Rating, Fte, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
