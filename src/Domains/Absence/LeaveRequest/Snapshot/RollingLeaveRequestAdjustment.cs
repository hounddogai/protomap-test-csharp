namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling leave request adjustment is in its life cycle.</summary>
public enum RollingLeaveRequestAdjustmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One rolling leave request adjustment, with what the planners need to act on it.</summary>
public sealed class RollingLeaveRequestAdjustment(string code, string title, bool visible, string country, double hours, string department, string team, string phone, string unit, string category, double ceiling, RollingLeaveRequestAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public string Country { get; set; } = country;
    public double Hours { get; set; } = hours;
    public string Department { get; set; } = department;
    public string Team { get; set; } = team;
    public string Phone { get; set; } = phone;
    public string Unit { get; set; } = unit;
    public string Category { get; set; } = category;
    public double Ceiling { get; set; } = ceiling;
    public RollingLeaveRequestAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling leave request adjustment in status proposed.</summary>
    public static RollingLeaveRequestAdjustment NewRollingLeaveRequestAdjustment(string code, string title)
    {
        return new RollingLeaveRequestAdjustment(
            code,
            title,
            false,
            "",
            0.0,
            "",
            "",
            "",
            "",
            "",
            0.0,
            RollingLeaveRequestAdjustmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingLeaveRequestAdjustmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingLeaveRequestAdjustmentStatus next)
    {
        switch (Status)
        {
            case RollingLeaveRequestAdjustmentStatus.Proposed:
                return next == RollingLeaveRequestAdjustmentStatus.Rejected || next == RollingLeaveRequestAdjustmentStatus.Withdrawn;
            case RollingLeaveRequestAdjustmentStatus.Approved:
                switch (next)
                {
                    case RollingLeaveRequestAdjustmentStatus.Rejected:
                        return true;
                }
                return false;
            case RollingLeaveRequestAdjustmentStatus.Rejected:
                return next == RollingLeaveRequestAdjustmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RollingLeaveRequestAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling leave request adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling leave request adjustment"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Hours - Ceiling, 2);
    }

    public RollingLeaveRequestAdjustment Copy()
    {
        return new RollingLeaveRequestAdjustment(
            Code,
            Title,
            Visible,
            Country,
            Hours,
            Department,
            Team,
            Phone,
            Unit,
            Category,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
