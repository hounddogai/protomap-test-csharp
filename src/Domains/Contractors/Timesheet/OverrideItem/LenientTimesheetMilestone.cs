namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient timesheet milestone is in its life cycle.</summary>
public enum LenientTimesheetMilestoneStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One lenient timesheet milestone, with what the planners need to act on it.</summary>
public sealed class LenientTimesheetMilestone(string code, string label, double minimum, string name, int version, string reviewer, LenientTimesheetMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string Reviewer { get; set; } = reviewer;
    public LenientTimesheetMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a lenient timesheet milestone in status new.</summary>
    public static LenientTimesheetMilestone NewLenientTimesheetMilestone(string code, string label)
    {
        return new LenientTimesheetMilestone(code, label, 0.0, "", 0, "", LenientTimesheetMilestoneStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientTimesheetMilestoneStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientTimesheetMilestoneStatus next)
    {
        switch (Status)
        {
            case LenientTimesheetMilestoneStatus.New:
                switch (next)
                {
                    case LenientTimesheetMilestoneStatus.Queued:
                    case LenientTimesheetMilestoneStatus.Running:
                        return true;
                }
                return false;
            case LenientTimesheetMilestoneStatus.Queued:
                switch (next)
                {
                    case LenientTimesheetMilestoneStatus.Running:
                    case LenientTimesheetMilestoneStatus.Done:
                        return true;
                }
                return false;
            case LenientTimesheetMilestoneStatus.Running:
                switch (next)
                {
                    case LenientTimesheetMilestoneStatus.Done:
                    case LenientTimesheetMilestoneStatus.Failed:
                        return true;
                }
                return false;
            case LenientTimesheetMilestoneStatus.Done:
                switch (next)
                {
                    case LenientTimesheetMilestoneStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientTimesheetMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient timesheet milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Minimum - (double)Version, 2);
    }

    public LenientTimesheetMilestone Copy()
    {
        return new LenientTimesheetMilestone(Code, Label, Minimum, Name, Version, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
