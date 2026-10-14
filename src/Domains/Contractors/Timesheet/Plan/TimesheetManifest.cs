namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet manifest is in its life cycle.</summary>
public enum TimesheetManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A timesheet manifest. Its code is unique within a tenant.</summary>
public sealed class TimesheetManifest(string code, string title, string category, string badge, double maximum, TimesheetManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Badge { get; set; } = badge;
    public double Maximum { get; set; } = maximum;
    public TimesheetManifestStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet manifest in status new.</summary>
    public static TimesheetManifest NewTimesheetManifest(string code, string title)
    {
        return new TimesheetManifest(code, title, "", "", 0.0, TimesheetManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetManifestStatus next)
    {
        switch (Status)
        {
            case TimesheetManifestStatus.New:
                switch (next)
                {
                    case TimesheetManifestStatus.Queued:
                    case TimesheetManifestStatus.Done:
                        return true;
                }
                return false;
            case TimesheetManifestStatus.Queued:
                return next == TimesheetManifestStatus.Done;
            case TimesheetManifestStatus.Running:
                switch (next)
                {
                    case TimesheetManifestStatus.Done:
                        return true;
                }
                return false;
            case TimesheetManifestStatus.Done:
                switch (next)
                {
                    case TimesheetManifestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public TimesheetManifest Copy()
    {
        return new TimesheetManifest(Code, Title, Category, Badge, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
