namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary leave request agreement is in its life cycle.</summary>
public enum PrimaryLeaveRequestAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A primary leave request agreement. Its code is unique within a tenant.</summary>
public sealed class PrimaryLeaveRequestAgreement(string code, string name, int attempts, int endsAt, double maximum, int tenureMonths, double threshold, string employeeId, double ratio, PrimaryLeaveRequestAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public int EndsAt { get; set; } = endsAt;
    public double Maximum { get; set; } = maximum;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Threshold { get; set; } = threshold;
    public string EmployeeId { get; set; } = employeeId;
    public double Ratio { get; set; } = ratio;
    public PrimaryLeaveRequestAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a primary leave request agreement in status new.</summary>
    public static PrimaryLeaveRequestAgreement NewPrimaryLeaveRequestAgreement(string code, string name)
    {
        return new PrimaryLeaveRequestAgreement(
            code,
            name,
            0,
            0,
            0.0,
            0,
            0.0,
            "",
            0.0,
            PrimaryLeaveRequestAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryLeaveRequestAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryLeaveRequestAgreementStatus next)
    {
        switch (Status)
        {
            case PrimaryLeaveRequestAgreementStatus.New:
                return next == PrimaryLeaveRequestAgreementStatus.Queued;
            case PrimaryLeaveRequestAgreementStatus.Queued:
                return next == PrimaryLeaveRequestAgreementStatus.Failed;
            case PrimaryLeaveRequestAgreementStatus.Running:
                return next == PrimaryLeaveRequestAgreementStatus.Done || next == PrimaryLeaveRequestAgreementStatus.Failed;
            case PrimaryLeaveRequestAgreementStatus.Done:
                return next == PrimaryLeaveRequestAgreementStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PrimaryLeaveRequestAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary leave request agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"primary leave request agreement"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Threshold - (double)Attempts, 2);
    }

    public PrimaryLeaveRequestAgreement Copy()
    {
        return new PrimaryLeaveRequestAgreement(
            Code,
            Name,
            Attempts,
            EndsAt,
            Maximum,
            TenureMonths,
            Threshold,
            EmployeeId,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
