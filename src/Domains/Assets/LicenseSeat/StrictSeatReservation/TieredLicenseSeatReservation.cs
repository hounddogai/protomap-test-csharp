namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered license seat reservation is in its life cycle.</summary>
public enum TieredLicenseSeatReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A tiered license seat reservation as the license seat team keeps it.</summary>
public sealed class TieredLicenseSeatReservation(string code, string label, string managerId, double rate, string category, double fte, string employeeId, TieredLicenseSeatReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public double Rate { get; set; } = rate;
    public string Category { get; set; } = category;
    public double Fte { get; set; } = fte;
    public string EmployeeId { get; set; } = employeeId;
    public TieredLicenseSeatReservationStatus Status { get; set; } = status;

    /// <summary>Starts a tiered license seat reservation in status proposed.</summary>
    public static TieredLicenseSeatReservation NewTieredLicenseSeatReservation(string code, string label)
    {
        return new TieredLicenseSeatReservation(
            code,
            label,
            "",
            0.0,
            "",
            0.0,
            "",
            TieredLicenseSeatReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredLicenseSeatReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredLicenseSeatReservationStatus next)
    {
        switch (Status)
        {
            case TieredLicenseSeatReservationStatus.Proposed:
                return next == TieredLicenseSeatReservationStatus.Rejected;
            case TieredLicenseSeatReservationStatus.Approved:
                return next == TieredLicenseSeatReservationStatus.Rejected;
            case TieredLicenseSeatReservationStatus.Rejected:
                return next == TieredLicenseSeatReservationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TieredLicenseSeatReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered license seat reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetExposure()
    {
        return Math.Round(Rate + Fte, 2);
    }

    public TieredLicenseSeatReservation Copy()
    {
        return new TieredLicenseSeatReservation(Code, Label, ManagerId, Rate, Category, Fte, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
