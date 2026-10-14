namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual action plan milestone is in its life cycle.</summary>
public enum ManualActionPlanMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a manual action plan milestone and its current status.</summary>
public sealed class ManualActionPlanMilestone(string code, string label, double cost, int expiresAt, double ceiling, int updatedAt, bool visible, string segment, double score, string location, string notes, ManualActionPlanMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Ceiling { get; set; } = ceiling;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Visible { get; set; } = visible;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public string Location { get; set; } = location;
    public string Notes { get; set; } = notes;
    public ManualActionPlanMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a manual action plan milestone in status idle.</summary>
    public static ManualActionPlanMilestone NewManualActionPlanMilestone(string code, string label)
    {
        return new ManualActionPlanMilestone(
            code,
            label,
            0.0,
            0,
            0.0,
            0,
            false,
            "",
            0.0,
            "",
            "",
            ManualActionPlanMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualActionPlanMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualActionPlanMilestoneStatus next)
    {
        switch (Status)
        {
            case ManualActionPlanMilestoneStatus.Idle:
                return next == ManualActionPlanMilestoneStatus.Ready || next == ManualActionPlanMilestoneStatus.Draining;
            case ManualActionPlanMilestoneStatus.Warming:
                return next == ManualActionPlanMilestoneStatus.Ready;
            case ManualActionPlanMilestoneStatus.Ready:
                switch (next)
                {
                    case ManualActionPlanMilestoneStatus.Draining:
                    case ManualActionPlanMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case ManualActionPlanMilestoneStatus.Draining:
                switch (next)
                {
                    case ManualActionPlanMilestoneStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualActionPlanMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual action plan milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"manual action plan milestone"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)UpdatedAt - Ceiling, 2);
    }

    public ManualActionPlanMilestone Copy()
    {
        return new ManualActionPlanMilestone(
            Code,
            Label,
            Cost,
            ExpiresAt,
            Ceiling,
            UpdatedAt,
            Visible,
            Segment,
            Score,
            Location,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
