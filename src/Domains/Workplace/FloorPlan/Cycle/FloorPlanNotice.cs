namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan notice is in its life cycle.</summary>
public enum FloorPlanNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a floor plan notice and its current status.</summary>
public sealed class FloorPlanNotice(string code, string title, int leaveDays, double ceiling, string notes, string locale, string team, FloorPlanNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public double Ceiling { get; set; } = ceiling;
    public string Notes { get; set; } = notes;
    public string Locale { get; set; } = locale;
    public string Team { get; set; } = team;
    public FloorPlanNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan notice in status proposed.</summary>
    public static FloorPlanNotice NewFloorPlanNotice(string code, string title)
    {
        return new FloorPlanNotice(code, title, 0, 0.0, "", "", "", FloorPlanNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanNoticeStatus next)
    {
        switch (Status)
        {
            case FloorPlanNoticeStatus.Proposed:
                return next == FloorPlanNoticeStatus.Approved || next == FloorPlanNoticeStatus.Rejected;
            case FloorPlanNoticeStatus.Approved:
                switch (next)
                {
                    case FloorPlanNoticeStatus.Rejected:
                    case FloorPlanNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case FloorPlanNoticeStatus.Rejected:
                return next == FloorPlanNoticeStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FloorPlanNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"floor plan notice"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)LeaveDays - Ceiling, 2);
    }

    public FloorPlanNotice Copy()
    {
        return new FloorPlanNotice(Code, Title, LeaveDays, Ceiling, Notes, Locale, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
