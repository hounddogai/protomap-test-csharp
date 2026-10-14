namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched attestation window is in its life cycle.</summary>
public enum BatchedAttestationWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A batched attestation window. Its code is unique within a tenant.</summary>
public sealed class BatchedAttestationWindow(string code, string label, string checksum, string description, string skill, double discount, double budget, string reviewer, string source, BatchedAttestationWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public string Description { get; set; } = description;
    public string Skill { get; set; } = skill;
    public double Discount { get; set; } = discount;
    public double Budget { get; set; } = budget;
    public string Reviewer { get; set; } = reviewer;
    public string Source { get; set; } = source;
    public BatchedAttestationWindowStatus Status { get; set; } = status;

    /// <summary>Starts a batched attestation window in status idle.</summary>
    public static BatchedAttestationWindow NewBatchedAttestationWindow(string code, string label)
    {
        return new BatchedAttestationWindow(
            code,
            label,
            "",
            "",
            "",
            0.0,
            0.0,
            "",
            "",
            BatchedAttestationWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAttestationWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAttestationWindowStatus next)
    {
        switch (Status)
        {
            case BatchedAttestationWindowStatus.Idle:
                return next == BatchedAttestationWindowStatus.Ready || next == BatchedAttestationWindowStatus.Stopped;
            case BatchedAttestationWindowStatus.Warming:
                switch (next)
                {
                    case BatchedAttestationWindowStatus.Ready:
                    case BatchedAttestationWindowStatus.Draining:
                        return true;
                }
                return false;
            case BatchedAttestationWindowStatus.Ready:
                switch (next)
                {
                    case BatchedAttestationWindowStatus.Draining:
                    case BatchedAttestationWindowStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedAttestationWindowStatus.Draining:
                switch (next)
                {
                    case BatchedAttestationWindowStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedAttestationWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched attestation window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Discount - Budget, 2);
    }

    public BatchedAttestationWindow Copy()
    {
        return new BatchedAttestationWindow(
            Code,
            Label,
            Checksum,
            Description,
            Skill,
            Discount,
            Budget,
            Reviewer,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
