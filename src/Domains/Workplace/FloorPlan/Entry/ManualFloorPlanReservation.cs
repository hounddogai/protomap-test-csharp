namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual floor plan reservation is in its life cycle.</summary>
public enum ManualFloorPlanReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a manual floor plan reservation and its current status.</summary>
public sealed class ManualFloorPlanReservation(string code, string name, double cost, string salaryBand, double margin, int graceDays, int windowDays, string notes, string skill, ManualFloorPlanReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string SalaryBand { get; set; } = salaryBand;
    public double Margin { get; set; } = margin;
    public int GraceDays { get; set; } = graceDays;
    public int WindowDays { get; set; } = windowDays;
    public string Notes { get; set; } = notes;
    public string Skill { get; set; } = skill;
    public ManualFloorPlanReservationStatus Status { get; set; } = status;

    /// <summary>Starts a manual floor plan reservation in status proposed.</summary>
    public static ManualFloorPlanReservation NewManualFloorPlanReservation(string code, string name)
    {
        return new ManualFloorPlanReservation(
            code,
            name,
            0.0,
            "",
            0.0,
            0,
            0,
            "",
            "",
            ManualFloorPlanReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualFloorPlanReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualFloorPlanReservationStatus next)
    {
        switch (Status)
        {
            case ManualFloorPlanReservationStatus.Proposed:
                switch (next)
                {
                    case ManualFloorPlanReservationStatus.Withdrawn:
                        return true;
                }
                return false;
            case ManualFloorPlanReservationStatus.Approved:
                return next == ManualFloorPlanReservationStatus.Rejected || next == ManualFloorPlanReservationStatus.Withdrawn;
            case ManualFloorPlanReservationStatus.Rejected:
                switch (next)
                {
                    case ManualFloorPlanReservationStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualFloorPlanReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual floor plan reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Margin * (double)GraceDays, 2);
    }

    public ManualFloorPlanReservation Copy()
    {
        return new ManualFloorPlanReservation(
            Code,
            Name,
            Cost,
            SalaryBand,
            Margin,
            GraceDays,
            WindowDays,
            Notes,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
