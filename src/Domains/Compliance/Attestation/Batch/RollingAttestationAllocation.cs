namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling attestation allocation is in its life cycle.</summary>
public enum RollingAttestationAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a rolling attestation allocation and its current status.</summary>
public sealed class RollingAttestationAllocation(string code, string label, string externalRef, double score, double budget, bool pinned, RollingAttestationAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public double Score { get; set; } = score;
    public double Budget { get; set; } = budget;
    public bool Pinned { get; set; } = pinned;
    public RollingAttestationAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a rolling attestation allocation in status open.</summary>
    public static RollingAttestationAllocation NewRollingAttestationAllocation(string code, string label)
    {
        return new RollingAttestationAllocation(
            code,
            label,
            "",
            0.0,
            0.0,
            false,
            RollingAttestationAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAttestationAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAttestationAllocationStatus next)
    {
        switch (Status)
        {
            case RollingAttestationAllocationStatus.Open:
                switch (next)
                {
                    case RollingAttestationAllocationStatus.Archived:
                        return true;
                }
                return false;
            case RollingAttestationAllocationStatus.InReview:
                switch (next)
                {
                    case RollingAttestationAllocationStatus.Resolved:
                    case RollingAttestationAllocationStatus.Archived:
                        return true;
                }
                return false;
            case RollingAttestationAllocationStatus.Resolved:
                switch (next)
                {
                    case RollingAttestationAllocationStatus.Reopened:
                    case RollingAttestationAllocationStatus.Archived:
                        return true;
                }
                return false;
            case RollingAttestationAllocationStatus.Reopened:
                return next == RollingAttestationAllocationStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RollingAttestationAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling attestation allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling attestation allocation"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Score + Budget, 2);
    }

    public RollingAttestationAllocation Copy()
    {
        return new RollingAttestationAllocation(Code, Label, ExternalRef, Score, Budget, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
