namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback timesheet plan is in its life cycle.</summary>
public enum FallbackTimesheetPlanStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback timesheet plan as the timesheet team keeps it.</summary>
public sealed class FallbackTimesheetPlan(string code, string name, string notes, double margin, double fte, double threshold, int priority, double cost, int capacity, string summary, FallbackTimesheetPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public double Margin { get; set; } = margin;
    public double Fte { get; set; } = fte;
    public double Threshold { get; set; } = threshold;
    public int Priority { get; set; } = priority;
    public double Cost { get; set; } = cost;
    public int Capacity { get; set; } = capacity;
    public string Summary { get; set; } = summary;
    public FallbackTimesheetPlanStatus Status { get; set; } = status;

    /// <summary>Starts a fallback timesheet plan in status open.</summary>
    public static FallbackTimesheetPlan NewFallbackTimesheetPlan(string code, string name)
    {
        return new FallbackTimesheetPlan(
            code,
            name,
            "",
            0.0,
            0.0,
            0.0,
            0,
            0.0,
            0,
            "",
            FallbackTimesheetPlanStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackTimesheetPlanStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackTimesheetPlanStatus next)
    {
        switch (Status)
        {
            case FallbackTimesheetPlanStatus.Open:
                switch (next)
                {
                    case FallbackTimesheetPlanStatus.InReview:
                    case FallbackTimesheetPlanStatus.Reopened:
                        return true;
                }
                return false;
            case FallbackTimesheetPlanStatus.InReview:
                switch (next)
                {
                    case FallbackTimesheetPlanStatus.Resolved:
                    case FallbackTimesheetPlanStatus.Archived:
                        return true;
                }
                return false;
            case FallbackTimesheetPlanStatus.Resolved:
                switch (next)
                {
                    case FallbackTimesheetPlanStatus.Reopened:
                        return true;
                }
                return false;
            case FallbackTimesheetPlanStatus.Reopened:
                switch (next)
                {
                    case FallbackTimesheetPlanStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackTimesheetPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback timesheet plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetTotal()
    {
        return Math.Round((double)Capacity * (double)Priority, 2);
    }

    public FallbackTimesheetPlan Copy()
    {
        return new FallbackTimesheetPlan(
            Code,
            Name,
            Notes,
            Margin,
            Fte,
            Threshold,
            Priority,
            Cost,
            Capacity,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
