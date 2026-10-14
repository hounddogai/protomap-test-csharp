namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip reservation is in its life cycle.</summary>
public enum PayslipReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One payslip reservation, with what the planners need to act on it.</summary>
public sealed class PayslipReservation(string code, string name, string costCenter, string managerId, int durationMinutes, double budget, int capacity, PayslipReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public string ManagerId { get; set; } = managerId;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Budget { get; set; } = budget;
    public int Capacity { get; set; } = capacity;
    public PayslipReservationStatus Status { get; set; } = status;

    /// <summary>Starts a payslip reservation in status requested.</summary>
    public static PayslipReservation NewPayslipReservation(string code, string name)
    {
        return new PayslipReservation(code, name, "", "", 0, 0.0, 0, PayslipReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipReservationStatus next)
    {
        switch (Status)
        {
            case PayslipReservationStatus.Requested:
                switch (next)
                {
                    case PayslipReservationStatus.Revoked:
                        return true;
                }
                return false;
            case PayslipReservationStatus.Granted:
                return next == PayslipReservationStatus.Revoked || next == PayslipReservationStatus.Expired;
            case PayslipReservationStatus.Revoked:
                return next == PayslipReservationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayslipReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"payslip reservation"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Budget * (double)Capacity, 2);
    }

    public PayslipReservation Copy()
    {
        return new PayslipReservation(Code, Name, CostCenter, ManagerId, DurationMinutes, Budget, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
