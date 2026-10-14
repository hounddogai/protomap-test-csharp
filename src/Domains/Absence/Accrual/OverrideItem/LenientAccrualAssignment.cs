namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient accrual assignment is in its life cycle.</summary>
public enum LenientAccrualAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A lenient accrual assignment. Its code is unique within a tenant.</summary>
public sealed class LenientAccrualAssignment(string code, string label, string timezone, int retries, string phone, double score, double fte, int priority, string badge, double amount, string contactEmail, LenientAccrualAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public int Retries { get; set; } = retries;
    public string Phone { get; set; } = phone;
    public double Score { get; set; } = score;
    public double Fte { get; set; } = fte;
    public int Priority { get; set; } = priority;
    public string Badge { get; set; } = badge;
    public double Amount { get; set; } = amount;
    public string ContactEmail { get; set; } = contactEmail;
    public LenientAccrualAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a lenient accrual assignment in status idle.</summary>
    public static LenientAccrualAssignment NewLenientAccrualAssignment(string code, string label)
    {
        return new LenientAccrualAssignment(
            code,
            label,
            "",
            0,
            "",
            0.0,
            0.0,
            0,
            "",
            0.0,
            "",
            LenientAccrualAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientAccrualAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientAccrualAssignmentStatus next)
    {
        switch (Status)
        {
            case LenientAccrualAssignmentStatus.Idle:
                switch (next)
                {
                    case LenientAccrualAssignmentStatus.Warming:
                        return true;
                }
                return false;
            case LenientAccrualAssignmentStatus.Warming:
                return next == LenientAccrualAssignmentStatus.Ready || next == LenientAccrualAssignmentStatus.Stopped;
            case LenientAccrualAssignmentStatus.Ready:
                return next == LenientAccrualAssignmentStatus.Draining || next == LenientAccrualAssignmentStatus.Stopped;
            case LenientAccrualAssignmentStatus.Draining:
                return next == LenientAccrualAssignmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LenientAccrualAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient accrual assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient accrual assignment"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Priority * (double)Retries, 2);
    }

    public LenientAccrualAssignment Copy()
    {
        return new LenientAccrualAssignment(
            Code,
            Label,
            Timezone,
            Retries,
            Phone,
            Score,
            Fte,
            Priority,
            Badge,
            Amount,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
