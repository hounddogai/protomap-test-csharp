namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet assignment is in its life cycle.</summary>
public enum TimesheetAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A timesheet assignment as the timesheet team keeps it.</summary>
public sealed class TimesheetAssignment(string code, string title, string externalRef, string salaryBand, int graceDays, string skill, string costCenter, string unit, double floorValue, double threshold, TimesheetAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public string SalaryBand { get; set; } = salaryBand;
    public int GraceDays { get; set; } = graceDays;
    public string Skill { get; set; } = skill;
    public string CostCenter { get; set; } = costCenter;
    public string Unit { get; set; } = unit;
    public double FloorValue { get; set; } = floorValue;
    public double Threshold { get; set; } = threshold;
    public TimesheetAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet assignment in status draft.</summary>
    public static TimesheetAssignment NewTimesheetAssignment(string code, string title)
    {
        return new TimesheetAssignment(
            code,
            title,
            "",
            "",
            0,
            "",
            "",
            "",
            0.0,
            0.0,
            TimesheetAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetAssignmentStatus next)
    {
        switch (Status)
        {
            case TimesheetAssignmentStatus.Draft:
                return next == TimesheetAssignmentStatus.Active || next == TimesheetAssignmentStatus.Suspended;
            case TimesheetAssignmentStatus.Pending:
                return next == TimesheetAssignmentStatus.Closed;
            case TimesheetAssignmentStatus.Active:
                switch (next)
                {
                    case TimesheetAssignmentStatus.Suspended:
                        return true;
                }
                return false;
            case TimesheetAssignmentStatus.Suspended:
                return next == TimesheetAssignmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TimesheetAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"timesheet assignment"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Threshold - FloorValue, 2);
    }

    public TimesheetAssignment Copy()
    {
        return new TimesheetAssignment(
            Code,
            Title,
            ExternalRef,
            SalaryBand,
            GraceDays,
            Skill,
            CostCenter,
            Unit,
            FloorValue,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
