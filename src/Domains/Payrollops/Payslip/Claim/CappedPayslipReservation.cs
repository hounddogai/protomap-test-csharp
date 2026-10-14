namespace Acme.Workforce.Payrollops.Payslip.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped payslip reservation is in its life cycle.</summary>
public enum CappedPayslipReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A capped payslip reservation. Its code is unique within a tenant.</summary>
public sealed class CappedPayslipReservation(string code, string label, int windowDays, int updatedAt, double rating, string reviewer, CappedPayslipReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Rating { get; set; } = rating;
    public string Reviewer { get; set; } = reviewer;
    public CappedPayslipReservationStatus Status { get; set; } = status;

    /// <summary>Starts a capped payslip reservation in status requested.</summary>
    public static CappedPayslipReservation NewCappedPayslipReservation(string code, string label)
    {
        return new CappedPayslipReservation(code, label, 0, 0, 0.0, "", CappedPayslipReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPayslipReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPayslipReservationStatus next)
    {
        switch (Status)
        {
            case CappedPayslipReservationStatus.Requested:
                return next == CappedPayslipReservationStatus.Revoked;
            case CappedPayslipReservationStatus.Granted:
                return next == CappedPayslipReservationStatus.Expired;
            case CappedPayslipReservationStatus.Revoked:
                switch (next)
                {
                    case CappedPayslipReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedPayslipReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped payslip reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"capped payslip reservation"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Rating - (double)UpdatedAt, 2);
    }

    public CappedPayslipReservation Copy()
    {
        return new CappedPayslipReservation(Code, Label, WindowDays, UpdatedAt, Rating, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
