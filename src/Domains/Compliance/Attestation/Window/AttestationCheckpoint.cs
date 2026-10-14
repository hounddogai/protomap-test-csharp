namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation checkpoint is in its life cycle.</summary>
public enum AttestationCheckpointStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One attestation checkpoint, with what the planners need to act on it.</summary>
public sealed class AttestationCheckpoint(string code, string label, double floorValue, bool visible, double budget, string currency, double minimum, string managerId, AttestationCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public bool Visible { get; set; } = visible;
    public double Budget { get; set; } = budget;
    public string Currency { get; set; } = currency;
    public double Minimum { get; set; } = minimum;
    public string ManagerId { get; set; } = managerId;
    public AttestationCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a attestation checkpoint in status open.</summary>
    public static AttestationCheckpoint NewAttestationCheckpoint(string code, string label)
    {
        return new AttestationCheckpoint(
            code,
            label,
            0.0,
            false,
            0.0,
            "",
            0.0,
            "",
            AttestationCheckpointStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationCheckpointStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationCheckpointStatus next)
    {
        switch (Status)
        {
            case AttestationCheckpointStatus.Open:
                return next == AttestationCheckpointStatus.Archived;
            case AttestationCheckpointStatus.InReview:
                switch (next)
                {
                    case AttestationCheckpointStatus.Reopened:
                    case AttestationCheckpointStatus.Archived:
                        return true;
                }
                return false;
            case AttestationCheckpointStatus.Resolved:
                return next == AttestationCheckpointStatus.Reopened || next == AttestationCheckpointStatus.Archived;
            case AttestationCheckpointStatus.Reopened:
                return next == AttestationCheckpointStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AttestationCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(FloorValue - Budget, 2);
    }

    public AttestationCheckpoint Copy()
    {
        return new AttestationCheckpoint(
            Code,
            Label,
            FloorValue,
            Visible,
            Budget,
            Currency,
            Minimum,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
