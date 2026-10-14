namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional payslip reservation is in its life cycle.</summary>
public enum RegionalPayslipReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One regional payslip reservation, with what the planners need to act on it.</summary>
public sealed class RegionalPayslipReservation(string code, string label, int limit, string salaryBand, int graceDays, int priority, string costCenter, double floorValue, string locale, double threshold, int revision, RegionalPayslipReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public string SalaryBand { get; set; } = salaryBand;
    public int GraceDays { get; set; } = graceDays;
    public int Priority { get; set; } = priority;
    public string CostCenter { get; set; } = costCenter;
    public double FloorValue { get; set; } = floorValue;
    public string Locale { get; set; } = locale;
    public double Threshold { get; set; } = threshold;
    public int Revision { get; set; } = revision;
    public RegionalPayslipReservationStatus Status { get; set; } = status;

    /// <summary>Starts a regional payslip reservation in status proposed.</summary>
    public static RegionalPayslipReservation NewRegionalPayslipReservation(string code, string label)
    {
        return new RegionalPayslipReservation(
            code,
            label,
            0,
            "",
            0,
            0,
            "",
            0.0,
            "",
            0.0,
            0,
            RegionalPayslipReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPayslipReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPayslipReservationStatus next)
    {
        switch (Status)
        {
            case RegionalPayslipReservationStatus.Proposed:
                return next == RegionalPayslipReservationStatus.Approved;
            case RegionalPayslipReservationStatus.Approved:
                return next == RegionalPayslipReservationStatus.Rejected;
            case RegionalPayslipReservationStatus.Rejected:
                return next == RegionalPayslipReservationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RegionalPayslipReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional payslip reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"regional payslip reservation"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Threshold + (double)Limit, 2);
    }

    public RegionalPayslipReservation Copy()
    {
        return new RegionalPayslipReservation(
            Code,
            Label,
            Limit,
            SalaryBand,
            GraceDays,
            Priority,
            CostCenter,
            FloorValue,
            Locale,
            Threshold,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
