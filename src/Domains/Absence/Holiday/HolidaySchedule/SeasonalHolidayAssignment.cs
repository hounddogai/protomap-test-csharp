namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal holiday assignment is in its life cycle.</summary>
public enum SeasonalHolidayAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A seasonal holiday assignment as the holiday team keeps it.</summary>
public sealed class SeasonalHolidayAssignment(string code, string label, double floorValue, double hours, int quantity, SeasonalHolidayAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public double Hours { get; set; } = hours;
    public int Quantity { get; set; } = quantity;
    public SeasonalHolidayAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal holiday assignment in status open.</summary>
    public static SeasonalHolidayAssignment NewSeasonalHolidayAssignment(string code, string label)
    {
        return new SeasonalHolidayAssignment(code, label, 0.0, 0.0, 0, SeasonalHolidayAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalHolidayAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalHolidayAssignmentStatus next)
    {
        switch (Status)
        {
            case SeasonalHolidayAssignmentStatus.Open:
                switch (next)
                {
                    case SeasonalHolidayAssignmentStatus.Resolved:
                    case SeasonalHolidayAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case SeasonalHolidayAssignmentStatus.InReview:
                switch (next)
                {
                    case SeasonalHolidayAssignmentStatus.Resolved:
                        return true;
                }
                return false;
            case SeasonalHolidayAssignmentStatus.Resolved:
                switch (next)
                {
                    case SeasonalHolidayAssignmentStatus.Reopened:
                        return true;
                }
                return false;
            case SeasonalHolidayAssignmentStatus.Reopened:
                switch (next)
                {
                    case SeasonalHolidayAssignmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalHolidayAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal holiday assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetLoad()
    {
        return Math.Round(FloorValue - (double)Quantity, 2);
    }

    public SeasonalHolidayAssignment Copy()
    {
        return new SeasonalHolidayAssignment(Code, Label, FloorValue, Hours, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
