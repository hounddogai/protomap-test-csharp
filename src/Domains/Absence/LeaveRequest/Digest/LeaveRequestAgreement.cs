namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request agreement is in its life cycle.</summary>
public enum LeaveRequestAgreementStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One leave request agreement, with what the planners need to act on it.</summary>
public sealed class LeaveRequestAgreement(string code, string name, int batchSize, double fte, double ceiling, int updatedAt, LeaveRequestAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public double Fte { get; set; } = fte;
    public double Ceiling { get; set; } = ceiling;
    public int UpdatedAt { get; set; } = updatedAt;
    public LeaveRequestAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a leave request agreement in status planned.</summary>
    public static LeaveRequestAgreement NewLeaveRequestAgreement(string code, string name)
    {
        return new LeaveRequestAgreement(code, name, 0, 0.0, 0.0, 0, LeaveRequestAgreementStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestAgreementStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestAgreementStatus next)
    {
        switch (Status)
        {
            case LeaveRequestAgreementStatus.Planned:
                return next == LeaveRequestAgreementStatus.Scheduled || next == LeaveRequestAgreementStatus.Cancelled;
            case LeaveRequestAgreementStatus.Scheduled:
                return next == LeaveRequestAgreementStatus.Completed || next == LeaveRequestAgreementStatus.Cancelled;
            case LeaveRequestAgreementStatus.InProgress:
                switch (next)
                {
                    case LeaveRequestAgreementStatus.Completed:
                        return true;
                }
                return false;
            case LeaveRequestAgreementStatus.Completed:
                return next == LeaveRequestAgreementStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LeaveRequestAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)UpdatedAt + Ceiling, 2);
    }

    public LeaveRequestAgreement Copy()
    {
        return new LeaveRequestAgreement(Code, Name, BatchSize, Fte, Ceiling, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
