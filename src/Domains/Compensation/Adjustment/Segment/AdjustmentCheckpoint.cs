namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment checkpoint is in its life cycle.</summary>
public enum AdjustmentCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adjustment checkpoint as the adjustment team keeps it.</summary>
public sealed class AdjustmentCheckpoint(string code, string name, double minimum, string ownerId, int sequence, string externalRef, string segment, string title, AdjustmentCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public string OwnerId { get; set; } = ownerId;
    public int Sequence { get; set; } = sequence;
    public string ExternalRef { get; set; } = externalRef;
    public string Segment { get; set; } = segment;
    public string Title { get; set; } = title;
    public AdjustmentCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment checkpoint in status proposed.</summary>
    public static AdjustmentCheckpoint NewAdjustmentCheckpoint(string code, string name)
    {
        return new AdjustmentCheckpoint(code, name, 0.0, "", 0, "", "", "", AdjustmentCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentCheckpointStatus next)
    {
        switch (Status)
        {
            case AdjustmentCheckpointStatus.Proposed:
                switch (next)
                {
                    case AdjustmentCheckpointStatus.Rejected:
                        return true;
                }
                return false;
            case AdjustmentCheckpointStatus.Approved:
                return next == AdjustmentCheckpointStatus.Withdrawn;
            case AdjustmentCheckpointStatus.Rejected:
                return next == AdjustmentCheckpointStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AdjustmentCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Sequence + Minimum, 2);
    }

    public AdjustmentCheckpoint Copy()
    {
        return new AdjustmentCheckpoint(Code, Name, Minimum, OwnerId, Sequence, ExternalRef, Segment, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
