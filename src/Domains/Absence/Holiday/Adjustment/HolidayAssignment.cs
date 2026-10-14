namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday assignment is in its life cycle.</summary>
public enum HolidayAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a holiday assignment and its current status.</summary>
public sealed class HolidayAssignment(string code, string name, double budget, string skill, string timezone, string externalRef, string team, string employeeId, HolidayAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string Skill { get; set; } = skill;
    public string Timezone { get; set; } = timezone;
    public string ExternalRef { get; set; } = externalRef;
    public string Team { get; set; } = team;
    public string EmployeeId { get; set; } = employeeId;
    public HolidayAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a holiday assignment in status proposed.</summary>
    public static HolidayAssignment NewHolidayAssignment(string code, string name)
    {
        return new HolidayAssignment(code, name, 0.0, "", "", "", "", "", HolidayAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayAssignmentStatus next)
    {
        switch (Status)
        {
            case HolidayAssignmentStatus.Proposed:
                switch (next)
                {
                    case HolidayAssignmentStatus.Approved:
                    case HolidayAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case HolidayAssignmentStatus.Approved:
                switch (next)
                {
                    case HolidayAssignmentStatus.Rejected:
                        return true;
                }
                return false;
            case HolidayAssignmentStatus.Rejected:
                switch (next)
                {
                    case HolidayAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public HolidayAssignment Copy()
    {
        return new HolidayAssignment(Code, Name, Budget, Skill, Timezone, ExternalRef, Team, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
