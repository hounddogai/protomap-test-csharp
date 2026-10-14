namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global attestation checkpoint is in its life cycle.</summary>
public enum GlobalAttestationCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a global attestation checkpoint and its current status.</summary>
public sealed class GlobalAttestationCheckpoint(string code, string label, double budget, bool visible, string currency, string skill, int attempts, bool pinned, int batchSize, int startsAt, GlobalAttestationCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public bool Visible { get; set; } = visible;
    public string Currency { get; set; } = currency;
    public string Skill { get; set; } = skill;
    public int Attempts { get; set; } = attempts;
    public bool Pinned { get; set; } = pinned;
    public int BatchSize { get; set; } = batchSize;
    public int StartsAt { get; set; } = startsAt;
    public GlobalAttestationCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a global attestation checkpoint in status idle.</summary>
    public static GlobalAttestationCheckpoint NewGlobalAttestationCheckpoint(string code, string label)
    {
        return new GlobalAttestationCheckpoint(
            code,
            label,
            0.0,
            false,
            "",
            "",
            0,
            false,
            0,
            0,
            GlobalAttestationCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAttestationCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAttestationCheckpointStatus next)
    {
        switch (Status)
        {
            case GlobalAttestationCheckpointStatus.Idle:
                switch (next)
                {
                    case GlobalAttestationCheckpointStatus.Warming:
                        return true;
                }
                return false;
            case GlobalAttestationCheckpointStatus.Warming:
                switch (next)
                {
                    case GlobalAttestationCheckpointStatus.Ready:
                    case GlobalAttestationCheckpointStatus.Draining:
                        return true;
                }
                return false;
            case GlobalAttestationCheckpointStatus.Ready:
                switch (next)
                {
                    case GlobalAttestationCheckpointStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalAttestationCheckpointStatus.Draining:
                return next == GlobalAttestationCheckpointStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(GlobalAttestationCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global attestation checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetLoad()
    {
        return Math.Round(Budget - (double)Attempts, 2);
    }

    public GlobalAttestationCheckpoint Copy()
    {
        return new GlobalAttestationCheckpoint(
            Code,
            Label,
            Budget,
            Visible,
            Currency,
            Skill,
            Attempts,
            Pinned,
            BatchSize,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
