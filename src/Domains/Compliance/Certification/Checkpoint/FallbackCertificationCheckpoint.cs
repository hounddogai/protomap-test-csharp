namespace Acme.Workforce.Compliance.Certification.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback certification checkpoint is in its life cycle.</summary>
public enum FallbackCertificationCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A fallback certification checkpoint. Its code is unique within a tenant.</summary>
public sealed class FallbackCertificationCheckpoint(string code, string title, double score, int sequence, string employeeId, FallbackCertificationCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public int Sequence { get; set; } = sequence;
    public string EmployeeId { get; set; } = employeeId;
    public FallbackCertificationCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a fallback certification checkpoint in status proposed.</summary>
    public static FallbackCertificationCheckpoint NewFallbackCertificationCheckpoint(string code, string title)
    {
        return new FallbackCertificationCheckpoint(
            code,
            title,
            0.0,
            0,
            "",
            FallbackCertificationCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackCertificationCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackCertificationCheckpointStatus next)
    {
        switch (Status)
        {
            case FallbackCertificationCheckpointStatus.Proposed:
                return next == FallbackCertificationCheckpointStatus.Approved;
            case FallbackCertificationCheckpointStatus.Approved:
                switch (next)
                {
                    case FallbackCertificationCheckpointStatus.Rejected:
                    case FallbackCertificationCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
            case FallbackCertificationCheckpointStatus.Rejected:
                switch (next)
                {
                    case FallbackCertificationCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackCertificationCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback certification checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Sequence * Score, 2);
    }

    public FallbackCertificationCheckpoint Copy()
    {
        return new FallbackCertificationCheckpoint(Code, Title, Score, Sequence, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
