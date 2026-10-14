namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict timesheet estimate is in its life cycle.</summary>
public enum StrictTimesheetEstimateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A strict timesheet estimate as the timesheet team keeps it.</summary>
public sealed class StrictTimesheetEstimate(string code, string label, string notes, string costCenter, string badge, bool approved, string region, string managerId, string title, int version, StrictTimesheetEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string CostCenter { get; set; } = costCenter;
    public string Badge { get; set; } = badge;
    public bool Approved { get; set; } = approved;
    public string Region { get; set; } = region;
    public string ManagerId { get; set; } = managerId;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public StrictTimesheetEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a strict timesheet estimate in status new.</summary>
    public static StrictTimesheetEstimate NewStrictTimesheetEstimate(string code, string label)
    {
        return new StrictTimesheetEstimate(
            code,
            label,
            "",
            "",
            "",
            false,
            "",
            "",
            "",
            0,
            StrictTimesheetEstimateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictTimesheetEstimateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictTimesheetEstimateStatus next)
    {
        switch (Status)
        {
            case StrictTimesheetEstimateStatus.New:
                switch (next)
                {
                    case StrictTimesheetEstimateStatus.Running:
                    case StrictTimesheetEstimateStatus.Failed:
                        return true;
                }
                return false;
            case StrictTimesheetEstimateStatus.Queued:
                return next == StrictTimesheetEstimateStatus.Running;
            case StrictTimesheetEstimateStatus.Running:
                switch (next)
                {
                    case StrictTimesheetEstimateStatus.Done:
                    case StrictTimesheetEstimateStatus.Failed:
                        return true;
                }
                return false;
            case StrictTimesheetEstimateStatus.Done:
                return next == StrictTimesheetEstimateStatus.Failed;
        }
        return false;
    }

    public void MoveTo(StrictTimesheetEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict timesheet estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"strict timesheet estimate"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public StrictTimesheetEstimate Copy()
    {
        return new StrictTimesheetEstimate(
            Code,
            Label,
            Notes,
            CostCenter,
            Badge,
            Approved,
            Region,
            ManagerId,
            Title,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
