namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty schedule is in its life cycle.</summary>
public enum WarrantyScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a warranty schedule and its current status.</summary>
public sealed class WarrantySchedule(string code, string label, string costCenter, string department, double ceiling, string checksum, double hours, string location, WarrantyScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public string Department { get; set; } = department;
    public double Ceiling { get; set; } = ceiling;
    public string Checksum { get; set; } = checksum;
    public double Hours { get; set; } = hours;
    public string Location { get; set; } = location;
    public WarrantyScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a warranty schedule in status proposed.</summary>
    public static WarrantySchedule NewWarrantySchedule(string code, string label)
    {
        return new WarrantySchedule(code, label, "", "", 0.0, "", 0.0, "", WarrantyScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyScheduleStatus next)
    {
        switch (Status)
        {
            case WarrantyScheduleStatus.Proposed:
                return next == WarrantyScheduleStatus.Rejected;
            case WarrantyScheduleStatus.Approved:
                return next == WarrantyScheduleStatus.Rejected || next == WarrantyScheduleStatus.Withdrawn;
            case WarrantyScheduleStatus.Rejected:
                switch (next)
                {
                    case WarrantyScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ceiling * Hours, 2);
    }

    public WarrantySchedule Copy()
    {
        return new WarrantySchedule(Code, Label, CostCenter, Department, Ceiling, Checksum, Hours, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
