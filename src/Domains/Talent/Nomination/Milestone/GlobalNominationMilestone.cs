namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global nomination milestone is in its life cycle.</summary>
public enum GlobalNominationMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a global nomination milestone and its current status.</summary>
public sealed class GlobalNominationMilestone(string code, string label, double ratio, bool approved, double maximum, bool archived, int expiresAt, double discount, bool active, int updatedAt, GlobalNominationMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public bool Approved { get; set; } = approved;
    public double Maximum { get; set; } = maximum;
    public bool Archived { get; set; } = archived;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Discount { get; set; } = discount;
    public bool Active { get; set; } = active;
    public int UpdatedAt { get; set; } = updatedAt;
    public GlobalNominationMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a global nomination milestone in status idle.</summary>
    public static GlobalNominationMilestone NewGlobalNominationMilestone(string code, string label)
    {
        return new GlobalNominationMilestone(
            code,
            label,
            0.0,
            false,
            0.0,
            false,
            0,
            0.0,
            false,
            0,
            GlobalNominationMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalNominationMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalNominationMilestoneStatus next)
    {
        switch (Status)
        {
            case GlobalNominationMilestoneStatus.Idle:
                switch (next)
                {
                    case GlobalNominationMilestoneStatus.Ready:
                    case GlobalNominationMilestoneStatus.Draining:
                        return true;
                }
                return false;
            case GlobalNominationMilestoneStatus.Warming:
                switch (next)
                {
                    case GlobalNominationMilestoneStatus.Ready:
                        return true;
                }
                return false;
            case GlobalNominationMilestoneStatus.Ready:
                switch (next)
                {
                    case GlobalNominationMilestoneStatus.Draining:
                    case GlobalNominationMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalNominationMilestoneStatus.Draining:
                return next == GlobalNominationMilestoneStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(GlobalNominationMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global nomination milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Discount + (double)ExpiresAt, 2);
    }

    public GlobalNominationMilestone Copy()
    {
        return new GlobalNominationMilestone(
            Code,
            Label,
            Ratio,
            Approved,
            Maximum,
            Archived,
            ExpiresAt,
            Discount,
            Active,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
