namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending engagement checkpoint is in its life cycle.</summary>
public enum PendingEngagementCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending engagement checkpoint. Its code is unique within a tenant.</summary>
public sealed class PendingEngagementCheckpoint(string code, string title, int sequence, double discount, string skill, string unit, PendingEngagementCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public double Discount { get; set; } = discount;
    public string Skill { get; set; } = skill;
    public string Unit { get; set; } = unit;
    public PendingEngagementCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a pending engagement checkpoint in status proposed.</summary>
    public static PendingEngagementCheckpoint NewPendingEngagementCheckpoint(string code, string title)
    {
        return new PendingEngagementCheckpoint(
            code,
            title,
            0,
            0.0,
            "",
            "",
            PendingEngagementCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingEngagementCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingEngagementCheckpointStatus next)
    {
        switch (Status)
        {
            case PendingEngagementCheckpointStatus.Proposed:
                return next == PendingEngagementCheckpointStatus.Rejected || next == PendingEngagementCheckpointStatus.Withdrawn;
            case PendingEngagementCheckpointStatus.Approved:
                switch (next)
                {
                    case PendingEngagementCheckpointStatus.Rejected:
                        return true;
                }
                return false;
            case PendingEngagementCheckpointStatus.Rejected:
                return next == PendingEngagementCheckpointStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PendingEngagementCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending engagement checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Sequence + Discount, 2);
    }

    public PendingEngagementCheckpoint Copy()
    {
        return new PendingEngagementCheckpoint(Code, Title, Sequence, Discount, Skill, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
